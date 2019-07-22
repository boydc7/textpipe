using System.Collections.Generic;
using System.Linq;

namespace TextWrangler.Models
{
    public class TargetField
    {
        private object _typedValue;

        public TargetField(string name, IEnumerable<(string Name, string Value)> sources)
        {
            Name = name;

            Sources = (sources ?? Enumerable.Empty<(string, string)>()).ToList().AsReadOnly();
        }

        public string Name { get; }
        public IReadOnlyList<(string Name, string Value)> Sources { get; }
        public string Value { get; set; }
        public string Type { get; set; }

        public object TypedValue
        {
            get => _typedValue ?? Value;
            set => _typedValue = value;
        }
    }
}
