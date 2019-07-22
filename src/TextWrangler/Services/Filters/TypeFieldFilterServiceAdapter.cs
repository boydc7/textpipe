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
                foreach (var targetField in targetRecord.Fields
                                                        .Where(f => !f.Type.IsNullOrEmpty()))
                {
                    try
                    {
                        targetField.TypedValue = targetField.Value.ConvertToType(Type.GetType(targetField.Type));
                    }
                    catch(Exception x) when(TextWranglerConfig.OnException(x, $"Could not convert target record [{recordNumber}] field [{targetField.Name}] to type specified [{targetField.Type}] - field value [{targetField.Value.Left(250)}]"))
                    {
                        throw;
                    }
                }

                yield return targetRecord;

                recordNumber++;
            }
        }

        public string Filter(string value, IEnumerable<string> filterNames)
            => _innerFieldFilterService.Filter(value, filterNames);
    }
}
