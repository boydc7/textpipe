using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Formatters
{
    public interface IFieldFormatter
    {
        IEnumerable<TargetRecord> Format(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration);
    }
}
