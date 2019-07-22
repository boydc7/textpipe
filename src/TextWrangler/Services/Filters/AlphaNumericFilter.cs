using System.Linq;
using TextWrangler.Extensions;
using TextWrangler.Models;

namespace TextWrangler.Services.Filters
{
    /// <summary>
    /// Verifies that a given value contains only alpha-numeric values. Throws a <see cref="TextWranglerFieldFilterException" />
    /// if the value contains any non-alpha-numeric character.
    /// </summary>
    public class AlphaNumericFilter : IFieldFilter
    {
        private const string _typeName = nameof(AlphaNumericFilter);

        private AlphaNumericFilter() { }

        public static AlphaNumericFilter Instance { get; } = new AlphaNumericFilter();

        public string Filter(string value)
        {
            if (!(value?.All(char.IsLetterOrDigit) ?? true))
            {
                throw new TextWranglerFieldFilterException(_typeName, $"Value has character(s) that are non-alphanumeric [{value.Left(250)}]");
            }

            return value;
        }
    }
}
