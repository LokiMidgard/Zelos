using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogOfWar
{
    static class LinqTaskExtensions
    {

        /// <summary>
        /// Returns the first task that statisfies the predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="p">The Predicate</param>
        /// <returns>The first Task that satisfied the predicate. Null if no Task satisfied it.</returns>
        public static async Task<Task> WhenAnyAsync<T>(this IEnumerable<Task<T>> c, Predicate<T> p)
        {
            var list = c.ToList();
            while (list.Count > 0)
            {
                var candidate = await Task.WhenAny(list);
                if (p(candidate.Result))
                    return candidate;
                list.Remove(candidate);
            }
            return null;
        }

        /// <summary>
        /// Returns the first task that statisfies the predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="c"></param>
        /// <param name="p">The Predicate</param>
        /// <returns>The first Task that satisfied the predicate. Null if no Task satisfied it.</returns>
        public static async Task<T> WhenAnyAsync<T>(this IEnumerable<T> c, Func<T, Task<bool>> p)
        {
            var lookup = c.ToDictionary(x => p(x));

            while (lookup.Count > 0)
            {
                var candidate = await Task.WhenAny(lookup.Keys);
                if (candidate.Result)
                    return lookup[candidate];
                lookup.Remove(candidate);
            }
            return default(T);
        }
    }
}
