using System.Collections.Generic;
using UnityEngine;
using Utility.Logging;

namespace Utility.Collections
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
        /// <remarks>
        /// <para>
        /// Using <see cref="IEnumerable{T}"/> instead of <see cref="List{T}"/> avoids unnecessary heap allocations
        /// when only enumeration is required. The compiler generates an iterator that yields a single element
        /// without creating an intermediate collection.
        /// </para>
        /// </remarks>
        public static IEnumerable<T> Single<T>(T item)
        {
            yield return item;
        }

        /// <summary>
        /// Returns a random element from the array.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        /// <param name="array">The source array.</param>
        /// <returns>A random element from the array.</returns>
        public static T GetRandomElement<T>(this T[] array)
        {
            if (array == null)
            {
                CustomLogger.LogWarning("GetRandomElement called on a null array.", null);
                return default;
            }

            if (array.Length == 0)
            {
                CustomLogger.LogWarning("GetRandomElement called on an empty array.", null);
                return default;
            }

            int index = Random.Range(0, array.Length);
            return array[index];
        }

        /// <summary>
        /// Returns a random element from a list.
        /// </summary>
        public static T GetRandomElement<T>(this IList<T> list)
        {
            if (list == null)
            {
                CustomLogger.LogWarning("GetRandomElement called on a null list.", null);
                return default;
            }

            if (list.Count == 0)
            {
                CustomLogger.LogWarning("GetRandomElement called on an empty list.", null);
                return default;
            }

            int index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}