using System.Collections;
using System.Collections.Generic;

namespace Utility.Collections
{
    /// <summary>
    /// Represents a 2D array flattened into a 1D array for efficient storage and access.
    /// </summary>
    /// <typeparam name="T">Type of elements stored in the array.</typeparam>
    public class FlattenedArray<T> : IEnumerable<T>
    {
        /// <summary>
        /// Width of the 2D array.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the 2D array.
        /// </summary>
        public int Height { get; }

        private readonly T[] _data;

        public FlattenedArray(int width, int height)
        {
            Width = width;
            Height = height;
            _data = new T[width * height];
        }

        /// <summary>
        /// Set the value at (x, y).
        /// </summary>
        public void Set(int x, int y, T value) => _data[ToIndex(x, y)] = value;

        /// <summary>
        /// Get the value at (x, y).
        /// </summary>
        public T Get(int x, int y) => _data[ToIndex(x, y)];

        /// <summary>
        /// Direct array access if needed.
        /// </summary>
        public T this[int x, int y]
        {
            get => _data[ToIndex(x, y)];
            set => _data[ToIndex(x, y)] = value;
        }

        /// <summary>
        /// Total number of elements in the array.
        /// </summary>
        public int Length => _data.Length;

        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in _data)
                yield return t;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Converts (x, y) into a flat array index.
        /// </summary>
        private int ToIndex(int x, int y) => y * Width + x;
    }
}