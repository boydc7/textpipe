using System;
using System.ComponentModel;
using TextWrangler.Configuration;

namespace TextWrangler.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
            => string.IsNullOrEmpty(source);

        public static object ConvertToType(this string source, Type toType)
        {
            if (TextWranglerConfig.TypeConverters.ContainsKey(toType))
            {
                return TextWranglerConfig.TypeConverters[toType](source);
            }

            var typeConverter = TypeDescriptor.GetConverter(toType);

            var convertedValue = typeConverter.ConvertFromString(source);

            return convertedValue;
        }

        public static string Left(this string source, int length)
            => string.IsNullOrEmpty(source)
                   ? string.Empty
                   : source.Length >= length
                       ? source.Substring(0, length)
                       : source;

        public static int ToInt(this string value, int defaultValue = 0)
        {
            if (IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return int.TryParse(value, out var i)
                       ? i
                       : defaultValue;
        }

        public static int Gz(this int value, int valueIfNegativeOrZero)
            => value > 0
                   ? value
                   : valueIfNegativeOrZero;
    }
}
