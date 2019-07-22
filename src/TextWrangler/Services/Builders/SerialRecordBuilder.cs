using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;
using TextWrangler.Services.Filters;
using TextWrangler.Services.Readers;

namespace TextWrangler.Services.Builders
{
    public class SerialRecordBuilder : IRecordBuilder
    {
        private readonly IFieldFilterService _fieldFilterService;

        public SerialRecordBuilder(IFieldFilterService fieldFilterService = null)
        {
            _fieldFilterService = fieldFilterService ?? TextWranglerConfig.DefaultFieldFilterService;
        }

        public static SerialRecordBuilder Default { get; } = new SerialRecordBuilder();

        /// <summary>
        /// Takes raw source records (likely via a <see cref="IRecordReader" />) serially and builds <see cref="TargetRecord" />s based on the
        /// <see cref="RecordConfiguration" /> and <see cref="IFieldFilterService" />
        /// </summary>
        /// <param name="sourceRecords">Raw field name/value map of source records</param>
        /// <param name="recordConfiguration">Configuration (<see cref="RecordConfiguration" />) for use with generating the resulting emumerable of <see cref="TargetRecord" /></param>
        /// <returns>Enumerable of <see cref="TargetRecord" /></returns>
        public IEnumerable<TargetRecord> Build(IEnumerable<IReadOnlyDictionary<string, string>> sourceRecords,
                                               RecordConfiguration recordConfiguration)
        {
            var recordNumber = 1;

            foreach (var sourceRecord in sourceRecords)
            {
                TargetRecord targetRecord = null;

                try
                {
                    targetRecord = new TargetRecord(recordConfiguration.Fields
                                                                       .Select(fc => new TargetField(fc.Name,
                                                                                                     fc.Sources?
                                                                                                         .Select(sourceFieldConfig =>
                                                                                                                 {
                                                                                                                     if (!sourceRecord.TryGetValue(sourceFieldConfig.Name, out var sourceFieldValue))
                                                                                                                     {
                                                                                                                         throw new TextWranglerRecordFieldConfigInvalidException(recordConfiguration.RecordTypeName,
                                                                                                                                                                                 fc.Name,
                                                                                                                                                                                 $"Source field [{sourceFieldConfig.Name}] does not exist in source record");
                                                                                                                     }

                                                                                                                     var filteredSourceValue = _fieldFilterService.Filter(sourceFieldValue, sourceFieldConfig.Filters);

                                                                                                                     return (Name: sourceFieldConfig.Name, Value: filteredSourceValue);
                                                                                                                 }))
                                                                                     {
                                                                                         Value = fc.Format,
                                                                                         Type = fc.Type
                                                                                     }));
                }
                catch(Exception x) when(TextWranglerConfig.OnException(x, $"Could not build target record from source record [{recordNumber}] - source record values [{string.Join("|", sourceRecord.Values).Left(250)}]"))
                {
                    throw;
                }

                if (targetRecord != null)
                {
                    yield return targetRecord;
                }

                recordNumber++;
            }
        }
    }
}
