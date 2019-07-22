using System.Collections.Generic;
using System.Linq;

namespace TextWrangler.Models
{
    public class TargetRecord
    {
        public TargetRecord(IEnumerable<TargetField> fields)
        {
            Fields = (fields ?? Enumerable.Empty<TargetField>()).ToList().AsReadOnly();
        }

        public IReadOnlyList<TargetField> Fields { get; }
    }
}
