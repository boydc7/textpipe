using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                PrintUsage();

                return;
            }

            var recordConfigName = args[0];
            var fileName = args[1];

            var limit = args.Length > 2
                            ? args[2].ToInt(int.MaxValue)
                            : int.MaxValue;

            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"Could not find file named [{fileName}]. Please check the path and try again.");

                return;
            }

            if (!File.Exists(TextWranglerConfig.TextWranglerConfigFile))
            {
                System.Console.WriteLine($"Could not find TextWranger config file [{TextWranglerConfig.TextWranglerConfigFile}].");

                return;
            }

            var log = LogManager.GetLogger("TextWrangler.Console.Program");

            try
            {
                var wrangler = TextWrangler.CreateDefault(recordConfigName, fileName);

                wrangler.Wrangle(limit);
            }
            catch(TextWranglerException twx)
            {
                log.LogWranglerError(twx);
            }
            catch(Exception x)
            {
                log.LogError(x, "Unhandled exception occurred");
            }

            System.Console.WriteLine("");
            System.Console.WriteLine("Press any key to end...");
            System.Console.ReadKey();
        }

        private static void PrintUsage()
        {
            System.Console.WriteLine("USAGE:");
            System.Console.WriteLine("wrangler <recordConfigName> <filename>");
            System.Console.WriteLine("");
            System.Console.WriteLine("PARAMS:");
            System.Console.WriteLine("- recordConfigName: The name of the record type to parse the given filename as. Must");
            System.Console.WriteLine("                    exist within the textwrangler.json config file");
            System.Console.WriteLine("- fileName:         Full absolute or relative path to the CSV file to process.");
            System.Console.WriteLine("");
        }
    }
}
