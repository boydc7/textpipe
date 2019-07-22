using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TextWrangler.Extensions;

namespace TextWrangler.Services.Readers
{
    public class ProgressLoggedRecordReader : IRecordReader
    {
        private const int _modCountToLogProgress = 25_000;

        private readonly IRecordReader _innerRecordReader;
        private readonly ILogger _logger = LogManager.GetLogger("ProgressLoggedRecordReader");

        public ProgressLoggedRecordReader(IRecordReader innerRecordReader)
        {
            _innerRecordReader = innerRecordReader;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _innerRecordReader?.Dispose();
        }

        public int CountRead => _innerRecordReader.CountRead;
        public int CountFail => _innerRecordReader.CountFail;

        /// <summary>
        /// Simply logs progress every 25k records to the configured ILogger for this type
        /// </summary>
        public IEnumerable<IReadOnlyDictionary<string, string>> GetRecords(int limit = int.MaxValue)
        {
            var timer = Stopwatch.StartNew();

            foreach (var record in _innerRecordReader.GetRecords(limit))
            {
                if (_innerRecordReader.CountRead % _modCountToLogProgress == 0)
                {
                    _logger.LogInformation($"Read [{_innerRecordReader.CountRead,0:N0}] records from source, [{_innerRecordReader.CountFail,0:N0}] failed reads, in [{timer.Elapsed:mm\\:ss}]");
                }

                yield return record;
            }

            timer.Stop();

            _logger.LogInformation($"Read [{_innerRecordReader.CountRead,0:N0}] total records from source, [{_innerRecordReader.CountFail,0:N0}] total failed reads, in [{timer.Elapsed:mm\\:ss}]");
        }
    }
}
