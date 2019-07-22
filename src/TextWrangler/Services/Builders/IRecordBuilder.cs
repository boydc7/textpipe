using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Builders
{
    public interface IRecordBuilder
    {
        IEnumerable<TargetRecord> Build(IEnumerable<IReadOnlyDictionary<string, string>> sourceRecords,
                                        RecordConfiguration recordConfiguration);
    }
}
