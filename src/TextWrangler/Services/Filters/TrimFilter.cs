using TextWrangler.Extensions;

namespace TextWrangler.Services.Filters
{
    public class TrimFilter : IFieldFilter
    {
        private TrimFilter() { }

        public static TrimFilter Instance { get; } = new TrimFilter();

        public string Filter(string value)
            => value.IsNullOrEmpty()
                   ? value
                   : value.Trim();
    }
}
