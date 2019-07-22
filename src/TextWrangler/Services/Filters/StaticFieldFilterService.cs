using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Filters
{
    /// <summary>
    /// <see cref="IFieldFilterService" /> concrete implementation that takes a static list of named <see cref="IFieldFilter" />
    /// implementations and applies the given filters to target records in an iterative fashion
    /// </summary>
    public class StaticFieldFilterService : IFieldFilterService
    {
        private readonly IReadOnlyDictionary<string, IFieldFilter> _filterMap;

        public StaticFieldFilterService(params (string Name, IFieldFilter Filter)[] filters)
            : this(filters?.Select(f => new KeyValuePair<string, IFieldFilter>(f.Name, f.Filter))) { }

        public StaticFieldFilterService(IEnumerable<KeyValuePair<string, IFieldFilter>> filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            _filterMap = ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, filters);
        }

        public bool FilterExists(string value)
            => _filterMap.ContainsKey(value);

        public IEnumerable<TargetRecord> Filter(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            var recordNumber = 1;

            foreach (var targetRecord in targetRecords)
            {
                var recordFiltered = false;

                try
                {
                    foreach (var targetField in targetRecord.Fields)
                    {
                        if (!recordConfiguration.TryGetFieldByName(targetField.Name, out var targetFieldConfiguration))
                        {
                            throw new TextWranglerInvalidTargetStateException(recordConfiguration.RecordTypeName, targetField.Name,
                                                                              $"Target field has target name [{targetField.Name}] that does not exist in configuration");
                        }

                        if (targetFieldConfiguration.Filters.IsNullOrEmpty())
                        {
                            continue;
                        }

                        targetField.Value = Filter(targetField.Value, targetFieldConfiguration.Filters);
                    }

                    recordFiltered = true;
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, $"Could not filter target record from formatted record [{recordNumber}]"))
                {
                    throw;
                }

                if (recordFiltered)
                {
                    yield return targetRecord;
                }
            }
        }

        public string Filter(string value, IEnumerable<string> filterNames)
        {
            if (filterNames == null)
            {
                return value;
            }

            var returnValue = value;

            foreach (var filterName in filterNames)
            {
                if (!_filterMap.TryGetValue(filterName, out var fieldFilter))
                {
                    throw new ArgumentOutOfRangeException(nameof(filterNames), $"No IFieldFilter named [{filterName}] is mapped.");
                }

                returnValue = fieldFilter.Filter(returnValue);
            }

            return returnValue;
        }
    }
}
