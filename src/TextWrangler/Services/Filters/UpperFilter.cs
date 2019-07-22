using TextWrangler.Extensions;

namespace TextWrangler.Services.Filters
{
    public class UpperFilter : IFieldFilter
    {
        private UpperFilter() { }

        public static UpperFilter Instance { get; } = new UpperFilter();

        public string Filter(string value)
            => value.IsNullOrEmpty()
                   ? value
                   : value.ToUpper();
    }
}
