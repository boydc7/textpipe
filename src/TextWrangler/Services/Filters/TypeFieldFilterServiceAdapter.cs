using System;
using System.Collections.Generic;
using System.Linq;
using TextWrangler.Configuration;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Filters
{
    /// <summary>
    /// <see cref="IFieldFilterService" /> concrete implementation that verifies a final target field value can be converted
    /// to the <see cref="TargetField.Type" /> type in addition to filtering through the given inner filter service
    /// </summary>
    public class TypeFieldFilterServiceAdapter : IFieldFilterService
    {
        private readonly IFieldFilterService _innerFieldFilterService;

        public TypeFieldFilterServiceAdapter(IFieldFilterService innerFieldFilterService)
        {
            _innerFieldFilterService = innerFieldFilterService ?? throw new ArgumentNullException(nameof(innerFieldFilterService));
        }

        public bool FilterExists(string value)
            => _innerFieldFilterService.FilterExists(value);

        public IEnumerable<TargetRecord> Filter(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
        {
            var recordNumber = 1;

            foreach (var targetRecord in _innerFieldFilterService.Filter(targetRecords, recordConfiguration))
            {
                var recordFiltered = false;

                try
                {
                    foreach (var targetField in targetRecord.Fields
                                                            .Where(f => !f.Type.IsNullOrEmpty()))
                    {
                        targetField.TypedValue = targetField.Value.ConvertToType(targetField.Type.GetSystemType());
                    }

                    recordFiltered = true;
                }
                catch(Exception x) when(!TextWranglerConfig.OnException(x, $"Could not convert target record [{recordNumber}] fields to types specified"))
                {
                    // OnException handler says not to rethrow, so keep on going, skipping this record
                }

                if (recordFiltered)
                {
                    yield return targetRecord;
                }

                recordNumber++;
            }
        }

        public string Filter(string value, IEnumerable<string> filterNames)
            => _innerFieldFilterService.Filter(value, filterNames);
    }
}
