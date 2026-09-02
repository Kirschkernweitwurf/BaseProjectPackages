using System;
using System.Collections.Generic;
using Base.UtilityPackage.Collections;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers what the flattened grid has to behave like to stand in for a two dimensional array: a
    /// cell keeps what was put in it, the enumeration order is row by row, and a size that cannot
    /// exist is refused at construction instead of throwing somewhere later.
    /// </summary>
    public sealed class FlattenedArrayTests
    {
        private const int Height = 2;
        private const int Width = 3;

        private FlattenedArray<int> _grid;

        /// <summary>Every test starts from the same empty grid.</summary>
        [SetUp]
        public void Build() => _grid = new FlattenedArray<int>(Width, Height);

        /// <summary>The grid reports the size it was built with.</summary>
        [Test]
        public void TheGridReportsTheSizeItWasBuiltWith()
        {
            Assert.That(_grid.Width, Is.EqualTo(Width));
            Assert.That(_grid.Height, Is.EqualTo(Height));
            Assert.That(_grid.Length, Is.EqualTo(Width * Height));
        }

        /// <summary>A value written to a cell comes back from that same cell.</summary>
        [Test]
        public void AStoredValueComesBackFromTheSameCell()
        {
            _grid[2, 1] = 42;

            Assert.That(_grid[2, 1], Is.EqualTo(42));
            Assert.That(_grid[0, 0], Is.EqualTo(0), "no other cell may be touched");
        }

        /// <summary>The named accessors have to address the same cell as the indexer.</summary>
        [Test]
        public void GetAndSetMatchTheIndexer()
        {
            _grid.Set(1, 1, 7);

            Assert.That(_grid.Get(1, 1), Is.EqualTo(7));
            Assert.That(_grid[1, 1], Is.EqualTo(7));
        }

        /// <summary>Enumeration walks a full row before it moves down.</summary>
        [Test]
        public void TheGridEnumeratesRowByRow()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    _grid[x, y] = y * 10 + x;
            }

            List<int> visited = new(_grid);

            Assert.That(visited, Is.EqualTo(new List<int> { 0, 1, 2, 10, 11, 12 }));
        }

        /// <summary>A negative size cannot describe a grid, so it is refused right away.</summary>
        [Test]
        public void ANegativeSizeIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlattenedArray<int>(-1, Height));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FlattenedArray<int>(Width, -1));
        }

        /// <summary>A grid with no cells is legal and simply holds nothing.</summary>
        [Test]
        public void AGridWithoutCellsIsEmpty()
        {
            FlattenedArray<int> empty = new(0, 0);

            Assert.That(empty.Length, Is.EqualTo(0));
            Assert.That(empty, Is.Empty);
        }
    }
}