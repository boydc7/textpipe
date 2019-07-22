using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Filters
{
    public interface IFieldFilterService
    {
        bool FilterExists(string value);
        string Filter(string value, IEnumerable<string> filterNames);
        IEnumerable<TargetRecord> Filter(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration);
    }
}
