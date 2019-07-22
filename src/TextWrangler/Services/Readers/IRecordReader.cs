using System;
using System.Collections.Generic;

namespace TextWrangler.Services.Readers
{
    public interface IRecordReader : IDisposable
    {
        IEnumerable<IReadOnlyDictionary<string, string>> GetRecords(int limit = int.MaxValue);
        int CountRead { get; }
        int CountFail { get; }
    }
}
