using System;
using System.Collections.Generic;

namespace homeControl.ClientApi.Server
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<Tuple<T1, T2>> PairWith<T1, T2>(this IEnumerable<T1> source, IEnumerable<T2> other)
        {
            Guard.DebugAssertArgumentNotNull(source, nameof(source));
            Guard.DebugAssertArgumentNotNull(other, nameof(other));

            using (var e1 = source.GetEnumerator())
            using (var e2 = other.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                {
                    yield return Tuple.Create(e1.Current, e2.Current);
                }
            }
        }
    }
}