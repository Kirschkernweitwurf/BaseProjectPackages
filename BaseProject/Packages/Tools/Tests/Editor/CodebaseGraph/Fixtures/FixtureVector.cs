using System;

namespace Base.ToolsPackage.Editor.Tests.CodebaseGraph.Fixtures
{
    /// <summary>
    /// Carries an indexer and a pair of operators. Both are named something else in metadata, Item and
    /// op_Equality, which is exactly where a scanner that only matches source names goes wrong.
    /// </summary>
    public readonly struct FixtureVector : IEquatable<FixtureVector>
    {
        private readonly int _x;
        private readonly int _y;

        /// <summary>Indexer, called Item in metadata and written as this in source.</summary>
        /// <param name="index">Which of the two values to read.</param>
        public int this[int index] => index == 0
            ? _x
            : _y;

        /// <summary>Creates a vector.</summary>
        /// <param name="x">First value.</param>
        /// <param name="y">Second value.</param>
        public FixtureVector(int x, int y)
        {
            _x = x;
            _y = y;
        }

        /// <summary>Compares two vectors.</summary>
        /// <param name="left">First vector.</param>
        /// <param name="right">Second vector.</param>
        /// <returns>True when both values match.</returns>
        public static bool operator ==(FixtureVector left, FixtureVector right) => left.Equals(right);

        /// <summary>Compares two vectors.</summary>
        /// <param name="left">First vector.</param>
        /// <param name="right">Second vector.</param>
        /// <returns>True when either value differs.</returns>
        public static bool operator !=(FixtureVector left, FixtureVector right) => !left.Equals(right);

        /// <inheritdoc/>
        public bool Equals(FixtureVector other) => _x == other._x && _y == other._y;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is FixtureVector other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(_x, _y);
    }
}