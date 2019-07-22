using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using TextWrangler.Extensions;
using TextWrangler.Models;
using TextWrangler.Services.Builders;
using TextWrangler.Services.Filters;
using TextWrangler.Services.Formatters;
using TextWrangler.Services.Writers;

namespace TextWrangler.Configuration
{
    public static class TextWranglerConfig
    {
        private static readonly ILogger _log = LogManager.GetLogger("TextWranglerConfig");
        private static string _textWranglerConfigFile;

        /// <summary>
        /// Full absolute or relative path to the config file to use for record type configuration
        /// </summary>
        public static string TextWranglerConfigFile
        {
            get => _textWranglerConfigFile ?? (_textWranglerConfigFile = Path.GetFullPath("textwrangler.json"));
            set => _textWranglerConfigFile = Path.GetFullPath(value);
        }

        /// <summary>
        /// Sets the default logging level for logging events/information
        /// </summary>
        public static LogLevel DefaultLogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Get or set the logging factory. By default a Console-only logging factory is used at the <see cref="DefaultLogLevel" />
        /// </summary>
        public static ILoggerFactory LogFactory { get; set; }

        public static IFieldFilterService DefaultFieldFilterService { get; set; }
            = new TypeFieldFilterServiceAdapter(new StaticFieldFilterService(("trim", TrimFilter.Instance),
                                                                             ("upper", UpperFilter.Instance),
                                                                             ("alphanumeric", AlphaNumericFilter.Instance),
                                                                             ("alpha", AlphaFilter.Instance),
                                                                             ("titlecase", TitleCaseFilter.Instance)));

        public static IFieldFormatter DefaultFieldFormatter { get; set; }
            = new CompositeFieldFormatter(new IFieldFormatter[]
                                          {
                                              SourceFieldIndexReplacementFormatter.Instance,
                                              StringDotFormatFormatter.Instance
                                          });

        public static IRecordBuilder DefaultRecordBuilder { get; set; } = SerialRecordBuilder.Default;

        public static IRecordWriter DefaultRecordWriter { get; set; } = LogRecordWriter.Instance;

        /// <summary>
        /// Default exception handler when processing records - return true from the handler to stop execution and fail, return false
        /// to continue processing records in the pipeline
        /// </summary>
        public static Func<Exception, string, bool> OnException { get; set; } = (ex, msg) =>
                                                                                {
                                                                                    switch (ex)
                                                                                    {
                                                                                        case null:
                                                                                            _log.LogError(msg);

                                                                                            break;

                                                                                        case TextWranglerException twx:
                                                                                            _log.LogWranglerError(twx);

                                                                                            break;

                                                                                        default:
                                                                                            _log.LogError(ex, msg ?? "Unhandled exception occurred");

                                                                                            break;
                                                                                    }

                                                                                    return FailOnRecordError;
                                                                                };

        public static bool FailOnRecordError { get; set; } = false;

        public static Dictionary<Type, Func<string, object>> TypeConverters { get; set; }
            = new Dictionary<Type, Func<string, object>>
              {
                  {
                      typeof(double), s => Convert.ToDouble(s)
                  },
                  {
                      typeof(int), s => Convert.ToInt32(s)
                  },
                  {
                      typeof(long), s => Convert.ToInt64(s)
                  },
                  {
                      typeof(DateTime), s => Convert.ToDateTime(s)
                  }
              };

        /// <summary>
        /// Factory that provides instances of the default configuration to be used for CSV readers/writers
        /// </summary>
        public static Func<CsvHelper.Configuration.Configuration> DefaultCsvConfigurationFactory =>
            () => new CsvHelper.Configuration.Configuration
                  {
                      HasHeaderRecord = true,
                      CultureInfo = CultureInfo.InvariantCulture,
                      DetectColumnCountChanges = false,
                      Delimiter = ",",
                      Escape = '"',
                      Quote = '"',
                      AllowComments = false,
                      Encoding = Encoding.UTF8,
                      IgnoreBlankLines = true,
                      BufferSize = 1024 * 10,
                      BadDataFound = rctx =>
                                     {
                                         var msg = $"Could not read CSV record [{rctx.RawRow}], BadDataFound. RawRecord [{rctx.RawRecord}]";

                                         if (OnException(null, msg))
                                         {
                                             throw new TextWranglerReaderException(msg);
                                         }
                                     },
                      MissingFieldFound = (headers, recordIndex, rctx) =>
                                          {
                                              var msg = $"Source filed not found in CSV reader, row number  [{recordIndex}], MissingFieldFound. RawRecord [{rctx.RawRecord}]";

                                              if (OnException(null, msg))
                                              {
                                                  throw new TextWranglerReaderException(msg);
                                              }
                                          },
                      ReadingExceptionOccurred = csvx => OnException(csvx, null),
                  };
    }
}
