using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// Provides helper methods for creating and manipulating enumerables, such as lists or arrays.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Wraps a single element into an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="item">The element to wrap.</param>
        /// <returns>An enumerable yielding only <paramref name="item"/>.</returns>
        /// <remarks>
        /// Using <see cref="IEnumerable{T}"/> instead of <see cref="List{T}"/> avoids unnecessary heap allocations
        /// when only enumeration is required. The compiler generates an iterator that yields a single element
        /// without creating an intermediate collection.
        /// </remarks>
        public static IEnumerable<T> Single<T>(T item)
        {
            yield return item;
        }

        /// <summary>
        /// Returns a random element from a list. Arrays bind here as well, since they implement
        /// <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="list">The source list.</param>
        /// <returns>A random element, or the default value if the list is null or empty.</returns>
        public static T GetRandomElement<T>(this IList<T> list)
        {
            if (list == null)
            {
                CustomLogger.LogWarning($"{nameof(GetRandomElement)} called on a null collection.", null);
                return default(T);
            }

            if (list.Count == 0)
            {
                CustomLogger.LogWarning($"{nameof(GetRandomElement)} called on an empty collection.", null);
                return default(T);
            }

            return list[Random.Range(0, list.Count)];
        }
    }
}