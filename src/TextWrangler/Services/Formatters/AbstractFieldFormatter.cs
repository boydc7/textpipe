using System;
using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Formatters
{
    public abstract class AbstractFieldFormatter : IFieldFormatter
    {
        private readonly string _formatterName;

        protected AbstractFieldFormatter()
        {
            _formatterName = GetType().Name;
        }

        protected abstract TargetRecord FormatRecord(TargetRecord record, RecordConfiguration recordConfiguration);

        /// <summary>
        /// Formats the given records for the given configuration.
        /// </summary>
        /// <param name="targetRecords">Lazily evaluated enumerable of populated <see cref="TargetRecord" /> records</param>
        /// <param name="recordConfiguration">The <see cref="RecordConfiguration" /> being used to transform these source/target records</param>
        /// <returns>Enumerable of <see cref="TargetRecord" /></returns>
        public IEnumerable<TargetRecord> Format(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            var recordNumber = 1;

            foreach (var targetRecord in targetRecords)
            {
                TargetRecord yieldRecord = null;

                try
                {
                    yieldRecord = FormatRecord(targetRecord, recordConfiguration);
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, $"Could not format target record in [{_formatterName}] from built record [{recordNumber}]"))
                {
                    throw;
                }

                if (yieldRecord != null)
                {
                    yield return yieldRecord;
                }

                recordNumber++;
            }
        }
    }
}
