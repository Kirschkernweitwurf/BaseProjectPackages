using System;
using System.Collections;
using System.Collections.Generic;

namespace Base.UtilityPackage.Collections
{
    /// <summary>
    /// Represents a 2D array flattened into a 1D array for efficient storage and access.
    /// </summary>
    /// <typeparam name="T">Type of elements stored in the array.</typeparam>
    public sealed class FlattenedArray<T> : IEnumerable<T>
    {
        private const string NonNegativeMessage = "Must be non-negative.";

        /// <summary>
        /// Width of the 2D array.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the 2D array.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Total number of elements in the array.
        /// </summary>
        public int Length => _data.Length;

        /// <summary>
        /// Direct access to the element at (x, y).
        /// </summary>
        public T this[int x, int y]
        {
            get => _data[ToIndex(x, y)];
            set => _data[ToIndex(x, y)] = value;
        }

        private readonly T[] _data;

        /// <summary>
        /// Creates a grid of the given size.
        /// </summary>
        /// <param name="width">Number of columns. Must be non-negative.</param>
        /// <param name="height">Number of rows. Must be non-negative.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="width"/> or <paramref name="height"/> is negative.
        /// </exception>
        public FlattenedArray(int width, int height)
        {
            if (width < 0)
                throw new ArgumentOutOfRangeException(nameof(width), width, NonNegativeMessage);

            if (height < 0)
                throw new ArgumentOutOfRangeException(nameof(height), height, NonNegativeMessage);

            Width = width;
            Height = height;
            _data = new T[width * height];
        }

        /// <summary>
        /// Returns the underlying array's enumerator, avoiding a custom iterator state machine.
        /// </summary>
        /// <returns>An enumerator over all elements in row-major order.</returns>
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();

        /// <summary>
        /// Sets the value at (x, y).
        /// </summary>
        /// <param name="x">Column index.</param>
        /// <param name="y">Row index.</param>
        /// <param name="value">The value to store.</param>
        public void Set(int x, int y, T value) => _data[ToIndex(x, y)] = value;

        /// <summary>
        /// Gets the value at (x, y).
        /// </summary>
        /// <param name="x">Column index.</param>
        /// <param name="y">Row index.</param>
        /// <returns>The stored value.</returns>
        public T Get(int x, int y) => _data[ToIndex(x, y)];

        private int ToIndex(int x, int y) => y * Width + x;
    }
}