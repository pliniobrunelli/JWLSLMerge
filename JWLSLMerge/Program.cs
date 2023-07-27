namespace JWLSLMerge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Obtém todos os arquivos .jwlibrary na pasta do executável
            string[] jwlibraryFiles = null;

            if (args.Length == 0)
            {
                jwlibraryFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.jwlibrary");
            }
            else
            {
                switch (args[0].ToLower().Trim())
                {
                    case "-help":
                        ShowHelp();
                        return;

                    case "-folder":
                    case "-files":
                        jwlibraryFiles = GetFiles(args);
                        break;

                    default:
                        break;
                }
            }

            if (jwlibraryFiles == null || jwlibraryFiles.Length < 2)
            {
                Console.WriteLine("Please make sure there are at least two .jwlibrary files in the executable folder.");
                Console.WriteLine("Type -help for more information.");
                return;
            }

            MergeService mergeService = new MergeService();
            mergeService.Message += MergeService_Message;
            mergeService.Run(args);
        }

        private static string[] GetFiles(string[] args)
        {
            if (args.Length == 1)
                Console.WriteLine("Invalid arguments. Type -help for more information.");

            if (args[0].ToLower().Equals("-folder"))
            {
                return Directory.GetFiles(args[1], "*.jwlibrary");
            }
            else
            {
                return args
                    .Skip(1)
                    .Where(p => File.Exists(p) && Path.GetExtension(p.ToLower()) == ".jwlibrary")
                    .ToArray();
            }
        }

        private static void ShowHelp()
        {
            //help
            Console.WriteLine("To merge all the .jwlibrary files, just place them in this same directory and run the command: JWLSLMerge.exe");
            Console.WriteLine("");
            Console.WriteLine("If you wish, you can define the location of the files through the -folder parameter followed by the directory where the files are located.");
            Console.WriteLine("Example: JWLSLMerge.exe -folder \"c:\\my backups\"");
            Console.WriteLine("");
            Console.WriteLine("If you want to specify the files you want to merge, use the -files parameter followed by the full path of each file.");
            Console.WriteLine("Example: JWLSLMerge.exe -files \"c:\\my backups\\theme_003.jwlibrary\" \"c:\\my backups\\theme_157.jwlibrary\"");
        }

        private static void MergeService_Message(object? sender, string e)
        {
            Console.WriteLine(e);
        }
    }
}