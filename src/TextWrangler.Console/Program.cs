﻿using System;
using System.IO;
using Microsoft.Extensions.Logging;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;
using TextWrangler.Services.Readers;
using TextWrangler.Services.Writers;

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
                            ? args[2].ToInt().Gz(int.MaxValue)
                            : int.MaxValue;

            var outputFile = args.Length > 3
                                 ? args[3]
                                 : null;

            // Verify the source file exists
            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"Could not find file named [{fileName}]. Please check the path and try again.");

                return;
            }

            // Verify the textwrangler.json file (or configured config file) exists
            if (!File.Exists(TextWranglerConfig.TextWranglerConfigFile))
            {
                System.Console.WriteLine($"Could not find TextWranger config file [{TextWranglerConfig.TextWranglerConfigFile}].");

                return;
            }

            // If outputFile is specified verify it does not exist, or get overwrite permission
            if (!outputFile.IsNullOrEmpty() && File.Exists(outputFile))
            {
                System.Console.WriteLine($"Output file [{outputFile}] already exists, if you continue it will be overwritten.");
                System.Console.WriteLine("To continue, press any key. To cancel, press CTRL+C");

                System.Console.ReadKey();

                File.Delete(outputFile);
            }

            // Log and wrangle away
            var log = LogManager.GetLogger("TextWrangler.Console.Program");

            log.LogInformation($"Creating wrangler to process recordConfigName [{recordConfigName}], inputFile [{fileName}], writing to [{(outputFile.IsNullOrEmpty() ? "LogWriter" : outputFile)}], limit of [{limit}]");

            try
            {
                using(var wrangler = new TextWrangler(recordConfigName,
                                                      new CsvRecordReader(fileName),
                                                      recordWriter: outputFile.IsNullOrEmpty()
                                                                        ? (IRecordWriter)LogRecordWriter.Instance
                                                                        : new CsvRecordWriter(outputFile)))
                {
                    wrangler.Wrangle(limit);
                }
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
            System.Console.WriteLine("wrangler <recordConfigName> <filename> [<limit>] [<outputFileName>]");
            System.Console.WriteLine("");
            System.Console.WriteLine("PARAMS:");

            System.Console.WriteLine(@"- recordConfigName:
Required.
The name of the record type to parse the given filename as. Must
exist within the textwrangler.json config file
");

            System.Console.WriteLine(@"- inputFileName:
Required.
Full absolute or relative path to the input CSV file to read.
");

            System.Console.WriteLine(@"- limit:
Optional.
Limit of records to read (attempt to process) from the inputFileName.
If omitted or <= 0, the entire file is read/processed.
");

            System.Console.WriteLine(@"- outputFileName:
Optional.
Full absolute or relative path to the CSV file to output target records to.
If omitted, records are output to the logger configured for the LogRecordWriter type (by default the console only).

");
        }
    }
}
