using System;

namespace Base.UtilityPackage.Randomization
{
    /// <summary>
    /// A reproducible random number generator. The same seed always produces the same sequence, in
    /// every session and on every platform, which is what makes a run replayable from a seed alone.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity's global generator is a single sequence that every system draws from, so an unrelated
    /// caller drawing one number shifts everything that follows. An instance of this class belongs
    /// to one system and nothing else can move it.
    /// </para>
    /// <para>
    /// The algorithm is a permuted congruential generator: a plain congruential state whose weak
    /// low bits are thrown away and whose output is rotated by the state's own high bits. It is
    /// small, fast and well distributed, but not cryptographically secure, so never use it where
    /// the next value must not be predictable.
    /// </para>
    /// </remarks>
    public sealed class SeededRandom : IRandomSource
    {
        private const ulong FirstMixConstant = 0xBF58476D1CE4E5B9UL;
        private const int FirstMixShift = 30;
        private const ulong GoldenGap = 0x9E3779B97F4A7C15UL;
        private const ulong Multiplier = 6364136223846793005UL;
        private const int OutputShift = 27;
        private const int RotationMask = 31;
        private const int RotationShift = 59;
        private const ulong SecondMixConstant = 0x94D049BB133111EBUL;
        private const int SecondMixShift = 27;
        private const int ThirdMixShift = 31;
        private const int XorShift = 18;

        /// <summary>The seed this generator was built from. Log it to be able to replay a run.</summary>
        public int Seed { get; }

        /// <summary>
        /// The raw generator state. Save it to store a run in progress and hand it back to
        /// <see cref="Restore"/> to carry on with the exact draws that were still to come.
        /// </summary>
        public ulong State { get; private set; }

        private readonly ulong _increment;

        /// <summary>Creates a generator for the given seed.</summary>
        /// <param name="seed">The seed the sequence is derived from.</param>
        public SeededRandom(int seed) : this(seed, 0) { }

        /// <summary>
        /// Creates a generator for the given seed on a stream of its own. Two generators sharing a
        /// seed but not a stream produce unrelated sequences, which is how one seed can feed
        /// several systems without any of them repeating another.
        /// </summary>
        /// <param name="seed">The seed the sequence is derived from.</param>
        /// <param name="stream">The stream index. Any value works as long as it is distinct.</param>
        public SeededRandom(int seed, int stream)
        {
            Seed = seed;
            _increment = (ulong)(uint)stream << 1 | 1UL;

            Initialize();
        }

        /// <inheritdoc/>
        public uint NextUInt()
        {
            unchecked
            {
                ulong previous = State;

                State = previous * Multiplier + _increment;

                // Built from the previous state, not the new one: the output permutation has to be
                // one step behind the state so the state itself is never handed out.
                uint shifted = (uint)((previous >> XorShift ^ previous) >> OutputShift);
                int rotation = (int)(previous >> RotationShift);

                return shifted >> rotation | shifted << (-rotation & RotationMask);
            }
        }

        /// <summary>Creates a generator on a seed that differs every time this is called.</summary>
        /// <returns>A generator whose <see cref="Seed"/> can be logged to replay the same run.</returns>
        public static SeededRandom Create() => new(CreateSeed());

        /// <summary>Produces a seed with no relation to the previous one.</summary>
        /// <returns>The new seed.</returns>
        public static int CreateSeed() => Guid.NewGuid().GetHashCode();

        /// <summary>Rewinds to the start of the sequence, so the same draws come out again.</summary>
        public void Reset() => Initialize();

        /// <summary>Continues from a state captured earlier through <see cref="State"/>.</summary>
        /// <param name="state">The state to continue from.</param>
        public void Restore(ulong state) => State = state;

        // Spreads the seed out before it is used. Seeds in practice are counters and level indices,
        // and neighboring values would otherwise start out in neighboring parts of the sequence.
        private static ulong Mix(ulong value)
        {
            unchecked
            {
                value += GoldenGap;
                value = (value ^ value >> FirstMixShift) * FirstMixConstant;
                value = (value ^ value >> SecondMixShift) * SecondMixConstant;

                return value ^ value >> ThirdMixShift;
            }
        }

        private void Initialize()
        {
            unchecked
            {
                State = 0UL;

                NextUInt();

                State += Mix((uint)Seed);

                NextUInt();
            }
        }
    }
}