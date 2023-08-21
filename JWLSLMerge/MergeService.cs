using JWLSLMerge.Data;
using JWLSLMerge.Data.Models;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Security.Cryptography;

namespace JWLSLMerge
{
    public class MergeService
    {
        public event EventHandler<string>? Message;
        private readonly string targetPath = null!;
        private readonly string targetDbFile = null!;
        private string lastModified = null!;

        public MergeService()
        {
            targetPath = Environment.GetTargetDirectory();
            targetDbFile = Environment.GetDbFile();
        }

        public void Run(string[] jwlibraryFiles)
        {
            try
            {
                //get userData.db
                sendMessage("Preparing database file.");

                JWDal dbMerged = new JWDal(targetDbFile);

                foreach (string file in jwlibraryFiles.Where(p => File.Exists(p)))
                {
                    sendMessage($"Reading {Path.GetFileName(file)} file.");

                    //unzip
                    string tempDir = Environment.GetTempDirectory();
                    ZipFile.ExtractToDirectory(file, tempDir, true);

                    string? dbFile = Directory.GetFiles(tempDir, "userData.db").FirstOrDefault();

                    if (!string.IsNullOrEmpty(dbFile))
                    {
                        JWDal dbSource = new JWDal(dbFile);
                        merge(dbMerged, dbSource);

                        //delete the source db
                        File.Delete(dbFile);
                    }

                    //copy source files to target directory
                    string[] files = Directory.GetFiles(tempDir, "*.*");

                    foreach (var item in files)
                    {
                        File.Move(item, Path.Combine(targetPath, Path.GetFileName(item)), true);
                    }
                }

                lastModified = dbMerged.SetLastModification();

                createManifestFile();

                createJWLibraryFile();
            }
            catch (Exception ex)
            {
                sendMessage($"An error occurred while processing. Detail: {ex.Message}");
            }
        }

        private void createJWLibraryFile()
        {
            //create a jwlibrary file
            sendMessage($"Creating jwlibrary file.");

            string jwFile = Path.Combine(Environment.GetMergedDirectory(), "merged.jwlibrary");
            if (File.Exists(jwFile)) File.Delete(jwFile);

            ZipFile.CreateFromDirectory(targetPath, jwFile);

            sendMessage($"Done. The file has been created in {jwFile}.");
        }

        private void createManifestFile()
        {
            //create a manifest.json
            sendMessage($"Creating manifest file.");

            Manifest manifest = new Manifest()
            {
                CreationDate = lastModified,
                Name = $"JWSLMerge_{lastModified}",
                UserDataBackup = new UserDataBackup()
                {
                    DatabaseName = "userData.db",
                    DeviceName = System.Environment.MachineName,
                    LastModifiedDate = lastModified,
                    SchemaVersion = 11,
                    Hash = GenerateDatabaseHash(targetDbFile)
                }
            };

            string jsonManifest = JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.None);

            string pathManifest = Path.Combine(targetPath, "manifest.json");
            if (File.Exists(pathManifest)) File.Delete(pathManifest);

            using (var sw = new StreamWriter(Path.Combine(targetPath, "manifest.json"), false))
            {
                sw.Write(jsonManifest);
            };
        }

        private void sendMessage(string message)
        {
            if (Message != null)
            {
                Message(this, message);
            }
        }

        private void merge(JWDal dbMerged, JWDal dbSource)
        {
            /* Location
             * InputField (depend on Location)
             * BookMark (depend on Location)
             * UserMark (depend on Location)
             * BlockRange (depend on UserMark)
             * Note (depends on Location and UserMark)
             * IndependentMedia
             * PlayListItem (depends on IndependentMedia)
             * PlayListItemIndependentMediaMap (depends on IndependentMedia and PlayListItem)
             * PlayListItemLocationMap (depends on Location and PlayListItem)
             * Tag
             * TagMap (depends on Tag, Location, PlaylistItem and Note)
             * PlaylistItemMarker (depends on PlayListItem)
             * PlaylistItemMarkerBibleVerseMap (depends on PlaylistItemMarker)
             * PlaylistItemMarkerParagraphMap (depends on PlaylistItemMarker)
             */

            //merge Location
            var t_Location = dbSource.TableList<Location>();

            foreach (var item in t_Location)
            {
                try
                {
                    var location1 = dbMerged.GetFirst<Location>(item, new string[] { "KeySymbol", "IssueTagNumber", "MepsLanguage", "BookNumber", "DocumentId", "Track", "Type" });
                    var location2 = dbMerged.GetFirst<Location>(item, new string[] { "BookNumber", "ChapterNumber", "KeySymbol", "MepsLanguage", "Type" }, false);
                    var location3 = dbMerged.GetFirst<Location>(item, new string[] { "KeySymbol", "IssueTagNumber", "MepsLanguage", "DocumentId", "Track", "Type" });

                    if (location1 == null && location2 == null && location3 == null)
                    {
                        //update with new id. necessary for new foreign keys
                        item.NewLocationId = dbMerged.ItemInsert<Location>(item);
                    }
                    else
                    {
                        item.NewLocationId = (location1?.LocationId ?? location2?.LocationId ?? location3?.LocationId) ?? 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //merge InputField
            var t_InputField_old = dbSource.TableList<InputField>();
            var t_InputField_new = t_InputField_old.Join(t_Location, i => i.LocationId, l => l.LocationId,
                (i, l) =>
                {
                    i.LocationId = l.NewLocationId;
                    return i;
                }).ToList();

            foreach (var item in t_InputField_new)
            {
                dbMerged.ItemInsert<InputField>(item);
            }

            //merge BookMark
            var t_Bookmark_old = dbSource.TableList<Bookmark>();
            var t_Bookmark_new = t_Bookmark_old.Join(t_Location, b => b.LocationId, l => l.LocationId,
                (b, l) =>
                {
                    b.LocationId = l.NewLocationId;
                    return b;
                }).ToList();

            foreach (var item in t_Bookmark_new)
            {
                dbMerged.ItemInsert<Bookmark>(item);
            }

            //merge Usermark
            var t_UserMark_old = dbSource.TableList<UserMark>();
            var t_UserMark_new = t_UserMark_old.Join(t_Location, u => u.LocationId, l => l.LocationId,
                (b, l) =>
                {
                    b.LocationId = l.NewLocationId;
                    return b;
                }).ToList();

            foreach (var item in t_UserMark_new)
            {
                item.NewUserMarkId = dbMerged.ItemInsert<UserMark>(item);
            }

            //merge BlockRange
            var t_BlockRange_old = dbSource.TableList<BlockRange>();
            var t_BlockRange_new = t_BlockRange_old.Join(t_UserMark_new, b => b.UserMarkId, u => u.UserMarkId,
                (b, u) =>
                {
                    b.UserMarkId = u.NewUserMarkId;
                    return b;
                }).ToList();

            foreach (var item in t_BlockRange_new)
            {
                dbMerged.ItemInsert<BlockRange>(item);
            }

            //merge Note
            var t_Note_old = dbSource.TableList<Note>();
            var t_Note_new = t_Note_old.Join(t_Location, n => n.LocationId, l => l.LocationId,
                (n, l) =>
                {
                    n.LocationId = l.NewLocationId;
                    return n;
                })
                .Join(t_UserMark_new, n => n.UserMarkId, u => u.UserMarkId,
                (n, u) =>
                {
                    n.UserMarkId = u.NewUserMarkId;
                    return n;
                }).ToList();

            foreach (var item in t_Note_new)
            {
                item.NewNoteId = dbMerged.ItemInsert<Note>(item);
            }

            //merge IndependentMedia
            var t_IndependentMedia = dbSource.TableList<IndependentMedia>();

            foreach (var item in t_IndependentMedia)
            {
                try
                {
                    var independentMedia1 = dbMerged.GetFirst<IndependentMedia>(item, new string[] { "FilePath" });

                    if (independentMedia1 == null)
                    {
                        //update with new id. necessary for new foreign keys
                        item.NewIndependentMediaId = dbMerged.ItemInsert<IndependentMedia>(item);
                    }
                    else
                    {
                        item.NewIndependentMediaId = independentMedia1.IndependentMediaId;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //merge PlayListItem
            var t_PlayListItem = dbSource.TableList<PlayListItem>();

            foreach (var item in t_PlayListItem)
            {
                //update with new id. necessary for new foreign keys
                item.NewPlaylistItemId = dbMerged.ItemInsert<PlayListItem>(item);
            }

            //merge PlayListItemIndependentMediaMap
            var t_PlayListItemIndependentMediaMap_old = dbSource.TableList<PlaylistItemIndependentMediaMap>();
            var t_PlayListItemIndependentMediaMap_new = t_PlayListItemIndependentMediaMap_old.Join(t_IndependentMedia, m => m.IndependentMediaId, i => i.IndependentMediaId,
                (m, i) =>
                {
                    m.IndependentMediaId = i.NewIndependentMediaId;
                    return m;
                })
                .Join(t_PlayListItem, m => m.PlaylistItemId, p => p.PlaylistItemId,
                (m, p) =>
                {
                    m.PlaylistItemId = p.NewPlaylistItemId;
                    return m;
                }).ToList();

            foreach (var item in t_PlayListItemIndependentMediaMap_new)
            {
                dbMerged.ItemInsert<PlaylistItemIndependentMediaMap>(item);
            }

            //merge PlaylistItemLocationMap
            var t_PlaylistItemLocationMap_old = dbSource.TableList<PlaylistItemLocationMap>();
            var t_PlaylistItemLocationMap_new = t_PlaylistItemLocationMap_old.Join(t_Location, m => m.LocationId, l => l.LocationId,
                (m, l) =>
                {
                    m.LocationId = l.NewLocationId;
                    return m;
                })
                .Join(t_PlayListItem, m => m.PlaylistItemId, p => p.PlaylistItemId,
                (m, p) =>
                {
                    m.PlaylistItemId = p.NewPlaylistItemId;
                    return m;
                }).ToList();

            foreach (var item in t_PlaylistItemLocationMap_new)
            {
                dbMerged.ItemInsert<PlaylistItemLocationMap>(item);
            }

            //merge Tag
            var t_Tag = dbSource.TableList<Tag>();

            foreach (var item in t_Tag)
            {
                try
                {
                    var tag1 = dbMerged.GetFirst<Tag>(item, new string[] { "Type", "Name" });

                    if (tag1 == null)
                    {
                        //update with new id. necessary for new foreign keys
                        item.NewTagId = dbMerged.ItemInsert<Tag>(item);
                    }
                    else
                    {
                        item.NewTagId = tag1.TagId;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //merge TagMap
            var t_TagMap_old = dbSource.TableList<TagMap>();
            var t_TagMap_new = t_TagMap_old.Join(t_Tag, m => m.TagId, t => t.TagId,
                (m, t) =>
                {
                    m.TagId = t.NewTagId;
                    return m;
                })
                //.Join(t_Location, m => m.LocationId, l => l.LocationId,
                //(m, l) =>
                //{
                //    m.LocationId = l.NewLocationId;
                //    return m;
                //})
                //.Join(t_Note_new, m => m.NoteId, n => n.NoteId,
                //(m, n) =>
                //{
                //    m.NoteId = n.NewNoteId;
                //    return m;
                //})
                .Join(t_PlayListItem, m => m.PlaylistItemId, p => p.PlaylistItemId,
                (m, p) =>
                {
                    m.PlaylistItemId = p.NewPlaylistItemId;
                    return m;
                }).ToList();

            foreach (var item in t_TagMap_new)
            {
                dbMerged.ItemInsert<TagMap>(item);
            }

            //merge PlaylistItemMarker
            var t_PlaylistItemMarker_old = dbSource.TableList<PlaylistItemMarker>();
            var t_PlaylistItemMarker_new = t_PlaylistItemMarker_old.Join(t_PlayListItem, m => m.PlaylistItemId, p => p.PlaylistItemId,
                (m, p) =>
                {
                    m.PlaylistItemId = p.NewPlaylistItemId;
                    return m;
                }).ToList();

            foreach (var item in t_PlaylistItemMarker_new)
            {
                item.NewPlaylistItemMarkerId = dbMerged.ItemInsert<PlaylistItemMarker>(item);
            }

            //merge PlaylistItemMarkerBibleVerseMap
            var t_PlaylistItemMarkerBibleVerseMap_old = dbSource.TableList<PlaylistItemMarkerBibleVerseMap>();
            var t_PlaylistItemMarkerBibleVerseMap_new = t_PlaylistItemMarkerBibleVerseMap_old.Join(t_PlaylistItemMarker_new, b => b.PlaylistItemMarkerId, m => m.PlaylistItemMarkerId,
                (b, m) =>
                {
                    b.PlaylistItemMarkerId = m.NewPlaylistItemMarkerId;
                    return b;
                }).ToList();

            foreach (var item in t_PlaylistItemMarkerBibleVerseMap_new)
            {
                dbMerged.ItemInsert<PlaylistItemMarkerBibleVerseMap>(item);
            }

            //merge PlaylistItemMarkerParagraphMap
            var t_PlaylistItemMarkerParagraphMap_old = dbSource.TableList<PlaylistItemMarkerParagraphMap>();
            var t_PlaylistItemMarkerParagraphMap_new = t_PlaylistItemMarkerParagraphMap_old.Join(t_PlaylistItemMarker_new, p => p.PlaylistItemMarkerId, m => m.PlaylistItemMarkerId,
                (p, m) =>
                {
                    p.PlaylistItemMarkerId = m.NewPlaylistItemMarkerId;
                    return p;
                }).ToList();

            foreach (var item in t_PlaylistItemMarkerParagraphMap_new)
            {
                dbMerged.ItemInsert<PlaylistItemMarkerParagraphMap>(item);
            }

            /*
             * sys tables?
             * Tag
             * PlaylistItemAccuracy
             */
        }

        private string GenerateDatabaseHash(string dbFile)
        {
            SHA256 sha256 = SHA256.Create();


            using var fs = new FileStream(dbFile, FileMode.Open);
            using var bs = new BufferedStream(fs);

            var hash = sha256.ComputeHash(bs);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
            //return string.Join("", hash.Select(b => $"{b:x2}").ToArray());
        }
    }
}