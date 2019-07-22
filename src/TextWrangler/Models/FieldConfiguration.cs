using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;

namespace TextWrangler.Models
{
    /// <summary>
    /// POCO representation of individual field elements within the <see cref="RecordConfiguration.Fields" /> list of a target <see cref="RecordConfiguration" /> file
    /// </summary>
    public class FieldConfiguration : BaseFieldConfiguration
    {
        private List<SourceFieldConfiguration> _sources;
        private Dictionary<string, SourceFieldConfiguration> _sourceFieldNameMap;

        /// <summary>
        /// Required.
        ///
        /// The format spec that is used to build the final value of the target field. Can include static text, delimitted source field references which
        /// may optionally be wrapped in a .NET string.Format spec.  The entire format value can also be wrapped by a format spec, however if you do so the
        /// <see cref="FieldConfiguration.Type" /> must also be specified and implement <see cref="IFormattable" />.
        ///
        /// Static text can simply be included/typed directly into the config
        ///
        /// Source field references must be delimitted by opening and closing angle brackets, and the source field referenced must exist in the <see cref="FieldConfiguration.Sources"> list.
        ///
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Optional.
        ///
        /// One or more source file/stream fields to be used in the given field to produce the output/target field value. If the source fomat allows for field names (i.e. CSV
        /// files with headers, JSON files, etc.), you can reference the fields by source field name. If the source field names are not available (i.e. CSV without a header,
        /// CSV with mixed-length fields without header names for all, etc.), you must reference the source field by index (zero based) within the given file/stream.
        /// </summary>
        public List<SourceFieldConfiguration> Sources
        {
            get => _sources;
            set
            {
                _sources = value;

                _sourceFieldNameMap = _sources.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        public bool TryGetSourceByName(string sourceName, out SourceFieldConfiguration sourceConfig)
            => _sourceFieldNameMap.TryGetValue(sourceName, out sourceConfig);
    }

    /// <summary>
    /// POCO representation of individual source field elements within the <see cref="FieldConfiguration.Sources" /> list of a <see cref="FieldConfiguration" /> in a target <see cref="RecordConfiguration" /> file
    /// </summary>
    public class SourceFieldConfiguration : BaseFieldConfiguration { }

    public abstract class BaseFieldConfiguration
    {
        private Type _systemType;

        /// <summary>
        /// Required.
        /// Name of the target field to export/output
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional. Required if the field is used within a String.Foramt spec in the field.
        ///
        /// The full .NET type name of the data type to cast the given value as. If omitted, field is treated as a string.
        ///
        /// If specified, the value must be convertible to the given type or an error will occur.
        /// </summary>
        /// <example>
        /// System.Double, System.Int32, System.Decimal, System.DateTime, System.Guid, etc.
        /// </example>
        public string Type { get; set; }

        /// <summary>
        /// Optional.
        ///
        /// Named filters to run the given value through before use.  Note that some filters implicitly validate specific requirements. For example, the
        /// 'alphanumeric' filter will throw an exeption if non-alpha-numeric values exist within the value.
        /// </summary>
        /// <example>
        /// trim , int, uppercase, alphanumeric
        /// </example>
        public List<string> Filters { get; set; }

        public Type GetSystemType() => _systemType ?? (_systemType = Type.GetSystemType());
    }
}
