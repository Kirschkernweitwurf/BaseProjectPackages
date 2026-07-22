using System;
using System.Collections;
using System.Collections.Generic;

namespace Base.UtilityPackage.Collections
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

        private readonly T[] _data;

        public FlattenedArray(int width, int height)
        {
            if (width < 0
                || height < 0)
                throw new ArgumentOutOfRangeException($"{nameof(width)}/{nameof(height)} must be non-negative.");

            Width = width;
            Height = height;
            _data = new T[width * height];
        }

        /// <summary>
        /// Returns the underlying array's enumerator, avoiding a custom iterator state machine.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        /// <summary>
        /// Set the value at (x, y).
        /// </summary>
        public void Set(int x, int y, T value) => _data[ToIndex(x, y)] = value;

        /// <summary>
        /// Get the value at (x, y).
        /// </summary>
        public T Get(int x, int y) => _data[ToIndex(x, y)];

        /// <summary>
        /// Converts (x, y) into a flat array index.
        /// </summary>
        private int ToIndex(int x, int y) => y * Width + x;
    }
}
