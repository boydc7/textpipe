using System;
using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Writers
{
    public interface IRecordWriter : IDisposable
    {
        IEnumerable<TargetRecord> Write(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration);
    }
}
