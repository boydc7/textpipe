using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Formatters
{
    /// <summary>
    /// Responsible for replacing delimitted (by opening and closing angle braces) source-field names within a field configuration format string with
    /// either the actual value of the field from the source (if it is just a static replacement), OR the index location of that source-field's value
    /// within the TargetRecord's Sources list (if it is going to be used as a format string).
    /// Delimitted format example would be:  "format": "&lt;Source Field Name Here&gt;"
    /// </summary>
    public class SourceFieldIndexReplacementFormatter : IFieldFormatter
    {
        private SourceFieldIndexReplacementFormatter() { }

        public static SourceFieldIndexReplacementFormatter Instance { get; } = new SourceFieldIndexReplacementFormatter();

        /// <summary>
        /// Formats the given records for the given configuration. Replaces delimited source-field names in the target fields with
        /// either the source field value (for non-format delimited fields) or the ordinal position of the source field in the configuration
        /// for use with string.format formatting
        /// </summary>
        /// <param name="targetRecords">Lazily evaluated enumerable of populated <see cref="TargetRecord" /> records</param>
        /// <param name="recordConfiguration">The <see cref="RecordConfiguration" /> being used to transform these source/target records</param>
        /// <returns>Enumerable of <see cref="TargetRecord" /></returns>
        public IEnumerable<TargetRecord> Format(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            var sourceFieldReplacementFormatMap = new Dictionary<string, (string Static, string Format)>(StringComparer.OrdinalIgnoreCase);

            foreach (var targetRecord in targetRecords)
            {
                var recordNumber = 1;
                var targetRecordFormatted = false;

                try
                {
                    foreach (var targetField in targetRecord.Fields
                                                            .Where(f => !f.Sources.IsNullOrEmpty()))
                    {
                        var sourceFieldIndex = 0;

                        foreach (var sourceValue in targetField.Sources)
                        {
                            // Build up a map of source field values to the delimited string we use to replace it with
                            // This is simply a bit of a memory-management/GC optimization so we don't have a ton of string objects getting created/destroyed on large files
                            if (!sourceFieldReplacementFormatMap.ContainsKey(sourceValue.Name))
                            {
                                sourceFieldReplacementFormatMap.Add(sourceValue.Name, (string.Concat("<", sourceValue.Name, ">"),
                                                                                       string.Concat("{<", sourceValue.Name, ">")));
                            }

                            // Another simple optimization of memory/gc at the expense of an extra lookup into the field value to avoid creating a new string unnecessarily
                            if (targetField.Value.IndexOf(sourceFieldReplacementFormatMap[sourceValue.Name].Static, StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                continue;
                            }

                            // Replace all instances of the format-string delimited source field name with the index of the source field within the
                            // target field config list (for those that are being used in a format-string case)
                            // OR the raw value (for those that are not being used in a format string case)
                            targetField.Value = targetField.Value
                                                           .Replace(sourceFieldReplacementFormatMap[sourceValue.Name].Format,
                                                                    string.Concat("{", sourceFieldIndex))
                                                           .Replace(sourceFieldReplacementFormatMap[sourceValue.Name].Static,
                                                                    sourceValue.Value);

                            sourceFieldIndex++;
                        }
                    }

                    targetRecordFormatted = true;
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, $"Could not format target record in [{GetType().Name}] from built record [{recordNumber}]"))
                {
                    // OnException handler says not to rethrow, so keep on going, skipping this record
                }

                if (targetRecordFormatted)
                {
                    yield return targetRecord;
                }

                recordNumber++;
            }
        }
    }
}
