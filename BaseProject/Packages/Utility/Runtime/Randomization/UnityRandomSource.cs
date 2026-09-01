using UnityEngine;

namespace Base.UtilityPackage.Randomization
{
    /// <summary>
    /// Exposes Unity's global generator as an <see cref="IRandomSource"/>, so code written against
    /// the interface can run on the engine sequence without carrying a seed of its own.
    /// </summary>
    /// <remarks>
    /// Holds no state. The sequence lives in Unity's own generator, which is why a single
    /// <see cref="Shared"/> instance is enough and why it survives a disabled domain reload
    /// unchanged: there is nothing here to reset.
    /// </remarks>
    public sealed class UnityRandomSource : IRandomSource
    {
        private const int HalfBits = 16;
        private const int HalfRange = 1 << HalfBits;

        /// <summary>The instance to pass around. Creating more of them serves no purpose.</summary>
        public static readonly UnityRandomSource Shared = new();

        /// <inheritdoc/>
        public uint NextUInt()
        {
            // Unity's integer range excludes its upper bound, so a full width draw would be short
            // by one value and biased. Two exact halves are assembled instead.
            uint high = (uint)Random.Range(0, HalfRange);
            uint low = (uint)Random.Range(0, HalfRange);

            return high << HalfBits | low;
        }
    }
}