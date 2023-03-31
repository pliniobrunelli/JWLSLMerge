using JWLSLMerge.Data;

namespace JWLSLMerge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide at least two .jwlibrary files with their full paths.");
                return;
            }

            MergeService mergeService = new MergeService();
            mergeService.Message += MergeService_Message;
            mergeService.Run(args);
        }

        private static void MergeService_Message(object? sender, string e)
        {
            Console.WriteLine(e);
        }
    }
}