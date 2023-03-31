namespace JWLSLMerge
{
    public class Environment
    {
        public static String ApplicationPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string GetDbFile()
        {
            string dboriginal = Path.Combine(Environment.ApplicationPath, "DB", "userData.db");
            string dbcopy = Path.Combine(GetTargetDirectory(false), Path.GetFileName(dboriginal));

            if (File.Exists(dboriginal))
            {
                File.Copy(dboriginal, dbcopy, true);
            }

            return dbcopy;
        }

        public static string GetTempDirectory(bool recreate = true)
        {
            return GetDirectory("temp", recreate);
        }

        public static string GetTargetDirectory(bool recreate = true)
        {
            return GetDirectory("target", recreate);
        }

        public static string GetMergedDirectory()
        {
            return GetDirectory("merged", true);
        }

        public static string GetDirectory(string folderName, bool recreate = true)
        {
            string tempdir = Path.Combine(Environment.ApplicationPath, folderName);

            if (recreate)
            {
                if (Directory.Exists(tempdir))
                {
                    Directory.Delete(tempdir, true);
                }

                Directory.CreateDirectory(tempdir);
            }

            return tempdir;
        }
    }
}
