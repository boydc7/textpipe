using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Writers
{
    /// <summary>
    /// Writes target records to the <see cref="ILogger" /> for this class. Should not be used for large numbers of records, as
    /// the output is fairly verbose and not very useful aside from debugging cases.
    /// </summary>
    public class LogRecordWriter : IRecordWriter
    {
        private readonly ILogger _logger = LogManager.GetLogger("LogRecordWriter");

        private LogRecordWriter() { }

        public static LogRecordWriter Instance { get; } = new LogRecordWriter();

        /// <summary>
        /// Writes targetRecords to configured logger for this type
        /// </summary>
        /// <param name="targetRecords"></param>
        /// <param name="recordConfiguration"></param>
        /// <returns>Lazy enumerable of <see cref="TargetRecord" /></returns>
        public IEnumerable<TargetRecord> Write(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            var recordNumber = 1;
            var logText = new StringBuilder();
            var maxFieldLength = int.MinValue;

            foreach (var targetRecord in targetRecords)
            {
                if (maxFieldLength <= 0)
                {
                    maxFieldLength = targetRecord.Fields.Max(f => f.Name.Length) + 2;
                }

                logText.AppendLine($"TARGET RECORD #[{recordNumber.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0')}]");
                logText.AppendLine("--------------------------------------------------------------------");

                foreach (var targetField in targetRecord.Fields)
                {
                    logText.AppendLine($"{targetField.Name.PadLeft(maxFieldLength)}:\t{targetField.TypedValue}");
                }

                _logger.LogInformation(logText.ToString());

                logText.Clear();

                yield return targetRecord;

                recordNumber++;
            }
        }

        public void Dispose() { }
    }
}
