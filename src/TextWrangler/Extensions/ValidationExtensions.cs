using System;
using System.Collections.Generic;
using TextWrangler.Models;

namespace TextWrangler.Extensions
{
    public static class ValidationExtensions
    {
        public static void Should<T>(this T source, Func<T, bool> must, string message)
        {
            if (must(source))
            {
                return;
            }

            throw new TextWranglerValidationException(message);
        }

        public static void ShouldAll<T>(this IEnumerable<T> sources, Func<T, bool> must,
                                        Func<T, string> messageFactory)
        {
            foreach (var source in sources)
            {
                Should(source, must, messageFactory(source));
            }
        }
    }
}
