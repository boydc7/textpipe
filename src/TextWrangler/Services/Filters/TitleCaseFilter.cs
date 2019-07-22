using System.Globalization;
using TextWrangler.Extensions;

namespace TextWrangler.Services.Filters
{
    public class TitleCaseFilter : IFieldFilter
    {
        private readonly TextInfo _textInfo;

        private TitleCaseFilter()
        {
            _textInfo = CultureInfo.CurrentCulture.TextInfo;
        }

        public static TitleCaseFilter Instance { get; } = new TitleCaseFilter();

        public string Filter(string value)
            => value.IsNullOrEmpty()
                   ? value
                   : _textInfo.ToTitleCase(value);
    }
}
