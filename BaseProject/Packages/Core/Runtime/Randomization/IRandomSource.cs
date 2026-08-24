namespace Base.CorePackage.Randomization
{
    /// <summary>
    /// A source of random bits. Everything this package draws goes through <see cref="NextUInt"/>,
    /// so ranges, chances, shuffles and point pickers are written once in
    /// <see cref="RandomSourceExtensions"/> and work with every source.
    /// </summary>
    /// <remarks>
    /// Take a dependency on this rather than on a concrete generator, so the same code can run on
    /// <see cref="SeededRandom"/> for a reproducible run, on <see cref="UnityRandomSource"/> for the
    /// engine's global sequence, or on a stub returning fixed values in a test.
    /// </remarks>
    public interface IRandomSource
    {
        /// <summary>
        /// Draws the next raw value. Every bit is expected to be uniformly distributed, since the
        /// helpers built on top of this rely on the high bits as much as on the low ones.
        /// </summary>
        /// <returns>A value anywhere in the full range of <see cref="uint"/>.</returns>
        uint NextUInt();
    }
}