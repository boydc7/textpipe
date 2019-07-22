using System.Collections.Generic;
using TextWrangler.Configuration;
using TextWrangler.Models;

namespace TextWrangler.Services.Writers
{
    public class NullRecordWriter : IRecordWriter
    {
        private NullRecordWriter() { }

        public static NullRecordWriter Instance { get; } = new NullRecordWriter();

        public IEnumerable<TargetRecord> Write(IEnumerable<TargetRecord> targetRecords, RecordConfiguration recordConfiguration)
            => targetRecords;

        public void Dispose() { }
    }
}
