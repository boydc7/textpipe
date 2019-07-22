using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Models;

namespace TextWrangler.Configuration
{
    /// <summary>
    /// POCO representation of a full record-type specification for mapping records
    /// from one source format to a target format.
    ///
    /// Generally represented in a JSON formatted config file named textwranger.json
    ///
    /// Use <see cref="RecordConfigurationBuilder" /> to easily build one of these from a
    /// properly formatted json config.
    /// </summary>
    public class RecordConfiguration
    {
        private IReadOnlyList<FieldConfiguration> _fields;
        private Dictionary<string, FieldConfiguration> _fieldNameMap;

        /// <summary>
        /// The unique name of the record specification within the configuration source
        /// </summary>
        public string RecordTypeName { get; set; }

        /// <summary>
        /// The field mappings specifications that define source-to-target field mapping,
        /// transforms, filters, etc.
        ///
        /// See <see cref="FieldConfiguration" />.
        /// </summary>
        public IReadOnlyList<FieldConfiguration> Fields
        {
            get => _fields;
            set
            {
                _fields = value;

                _fieldNameMap = _fields.ToDictionary(fc => fc.Name, StringComparer.OrdinalIgnoreCase);
            }
        }

        public bool TryGetFieldByName(string fieldName, out FieldConfiguration fieldConfiguration)
            => _fieldNameMap.TryGetValue(fieldName, out fieldConfiguration);
    }
}
