using System;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Formatters
{
    /// <summary>
    /// Responsible for running a target field value through string.Format() with the field's sources
    /// list as indexed parameters into the format string. Usually used in conjunction with the
    /// <see cref="SourceFieldIndexReplacementFormatter" /> formatter
    /// </summary>
    public class StringDotFormatFormatter : AbstractFieldFormatter
    {
        private StringDotFormatFormatter() { }

        public static StringDotFormatFormatter Instance { get; } = new StringDotFormatFormatter();

        /// <summary>
        /// Performs a string.format on the target field values in each of the records, passing the source field values
        /// from the RecordConfiguration source configuration as the format args.
        /// </summary>
        protected override TargetRecord FormatRecord(TargetRecord targetRecord, RecordConfiguration recordConfiguration)
        {
            foreach (var targetField in targetRecord.Fields)
            {
                if (targetField.Value.IndexOf('{') < 0)
                {
                    continue;
                }

                try
                {
                    targetField.Value = string.Format(targetField.Value, targetField.Sources
                                                                                    .Select(s =>
                                                                                            {
                                                                                                if (!recordConfiguration.TryGetFieldByName(targetField.Name, out var targetConfig))
                                                                                                {
                                                                                                    throw new TextWranglerInvalidTargetStateException(recordConfiguration.RecordTypeName, targetField.Name,
                                                                                                                                                      $"Target field has target name [{targetField.Name}] that does not exist in configuration");
                                                                                                }

                                                                                                if (!targetConfig.TryGetSourceByName(s.Name, out var sourceConfig))
                                                                                                {
                                                                                                    throw new TextWranglerInvalidTargetStateException(recordConfiguration.RecordTypeName, targetField.Name,
                                                                                                                                                      $"Target field [{targetField.Name}] has source name [{s.Name}] that does not exist in configuration");
                                                                                                }

                                                                                                if (sourceConfig.Type.IsNullOrEmpty())
                                                                                                {
                                                                                                    return s.Value;
                                                                                                }

                                                                                                var typedValue = s.Value.ConvertToType(sourceConfig.GetSystemType());

                                                                                                return typedValue;
                                                                                            })
                                                                                    .ToArray());
                }
                catch(FormatException fx) when(fx.Message.IndexOf("Input string was not in", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new TextWranglerRecordFieldConfigInvalidException(recordConfiguration.RecordTypeName, targetField.Name,
                                                                            $"Target field [{targetField.Name}] has invalid format string [{targetField.Value}]",
                                                                            fx);
                }
            }

            return targetRecord;
        }
    }
}
