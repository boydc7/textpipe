using System.Linq;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Filters
{
    /// <summary>
    /// Verifies that a given value contains only letter values. Throws a <see cref="TextWranglerFieldFilterException" />
    /// if the value contains any non-alpha characters.
    /// </summary>
    public class AlphaFilter : IFieldFilter
    {
        private const string _typeName = nameof(AlphaFilter);

        private AlphaFilter() { }

        public static AlphaFilter Instance { get; } = new AlphaFilter();

        public string Filter(string value)
        {
            if (!(value?.All(char.IsLetter) ?? true))
            {
                throw new TextWranglerFieldFilterException(_typeName, $"Value has character(s) that are non-alpha [{value.Left(250)}]");
            }

            return value;
        }
    }
}
