using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Services;
using TextWrangler.Services.Builders;
using TextWrangler.Services.Filters;
using TextWrangler.Services.Formatters;
using TextWrangler.Services.Readers;
using TextWrangler.Services.Writers;

namespace TextWrangler
{
    public class TextWrangler : IDisposable, ITextWrangler
    {
        private readonly IRecordReader _recordReader;
        private readonly IRecordBuilder _recordBuilder;
        private readonly IRecordWriter _recordWriter;
        private readonly IFieldFormatter _fieldFormatter;
        private readonly RecordConfiguration _recordConfiguration;
        private readonly IFieldFilterService _fieldFilterService;
        private readonly ILogger _logger;

        private bool _disposed;

        public TextWrangler(string recordConfigName,
                            IRecordReader recordReader,
                            IRecordBuilder recordBuilder = null,
                            IFieldFormatter fieldFormatter = null,
                            IFieldFilterService fieldFilterService = null,
                            IRecordWriter recordWriter = null)
            : this(RecordConfigurationBuilder.Build(recordConfigName), recordReader,
                   recordBuilder, fieldFormatter, fieldFilterService, recordWriter) { }

        public TextWrangler(RecordConfiguration recordConfiguration,
                            IRecordReader recordReader,
                            IRecordBuilder recordBuilder = null,
                            IFieldFormatter fieldFormatter = null,
                            IFieldFilterService fieldFilterService = null,
                            IRecordWriter recordWriter = null)
        {
            _recordConfiguration = recordConfiguration ?? throw new ArgumentNullException(nameof(recordReader));
            _recordReader = recordReader ?? throw new ArgumentNullException(nameof(recordReader));

            // Validate the configuration
            RecordConfigurationValidator.Instance.Validate(_recordConfiguration, _fieldFilterService);

            _recordBuilder = recordBuilder ?? TextWranglerConfig.DefaultRecordBuilder;
            _fieldFormatter = fieldFormatter ?? TextWranglerConfig.DefaultFieldFormatter;
            _fieldFilterService = fieldFilterService ?? TextWranglerConfig.DefaultFieldFilterService;
            _recordWriter = recordWriter ?? TextWranglerConfig.DefaultRecordWriter;

            _logger = LogManager.GetLogger(GetType().Name);
        }

        public void Wrangle(int limit = int.MaxValue)
        {
            var stopWatch = Stopwatch.StartNew();

            // TODO: Document...
            // How to run
            // Assumptions:
            // ProductNumber pseudo reg-ex implies alphanumeric, but CSV says otherwise...I'm assuming the ProductNumber can support non-alphanumeric characters - but to change that use a filter that validates or stips them

            _logger.LogInformation($"Starting Wrangling for recordType [{_recordConfiguration.RecordTypeName}]");

            // Simple lazy pipeline
            // Get source records, build target records, format the built targets, filter the final result, write the records...
            var countProcessed = _recordReader.GetRecords(limit)
                                              .Then(source => _recordBuilder.Build(source, _recordConfiguration))
                                              .Then(built => _fieldFormatter.Format(built, _recordConfiguration))
                                              .Then(format => _fieldFilterService.Filter(format, _recordConfiguration))
                                              .Then(filter => _recordWriter.Write(filter, _recordConfiguration))
                                              .Count();

            stopWatch.Stop();

            _logger.LogInformation($"Finished Wrangling of [{countProcessed}] target records in [{stopWatch.Elapsed:mm\\:ss}]");
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _recordReader?.Dispose();
            _recordWriter?.Dispose();

            _disposed = true;
        }
    }
}
