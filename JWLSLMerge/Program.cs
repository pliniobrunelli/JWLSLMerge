using JWLSLMerge.Data;
using System;
using System.IO;

namespace JWLSLMerge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Obtém o caminho da pasta do executável
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;

            // Obtém todos os arquivos .jwlibrary na pasta do executável
            string[] jwlibraryFiles = Directory.GetFiles(executablePath, "*.jwlibrary");

            if (jwlibraryFiles.Length < 2)
            {
                Console.WriteLine("Please make sure there are at least two .jwlibrary files in the executable folder.");
                return;
            }

            MergeService mergeService = new MergeService();
            mergeService.Message += MergeService_Message;
            mergeService.Run(jwlibraryFiles);
        }

        private static void MergeService_Message(object? sender, string e)
        {
            Console.WriteLine(e);
        }
    }
}
