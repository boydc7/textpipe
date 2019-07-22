using System;
using System.Collections.Generic;

namespace TextWrangler.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IReadOnlyList<T> source)
            => source == null || source.Count == 0;

        public static IEnumerable<T> AsEnumerable<T>(this T source)
        {
            yield return source;
        }

        public static IEnumerable<TOut> Then<T, TOut>(this IEnumerable<T> sources, Func<IEnumerable<T>, IEnumerable<TOut>> then,
                                                      Func<T, TOut, Exception, bool> onError = null)
            => then(sources);

        public static void Each<T>(this IEnumerable<T> sources, Action<T> each)
        {
            foreach (var source in sources)
            {
                each(source);
            }
        }
    }
}
