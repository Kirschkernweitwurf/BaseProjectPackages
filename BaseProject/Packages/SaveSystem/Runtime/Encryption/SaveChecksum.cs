namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// FNV-1a hash of a save payload, stored in the file header so damage is caught while reading the
    /// header instead of surfacing later as a strange parse error or, worse, as silently wrong state.
    /// </summary>
    /// <remarks>
    /// This detects damage, not tampering. Anyone who edits a save can recompute the value; use
    /// encryption for the other problem.
    /// </remarks>
    public static class SaveChecksum
    {
        /// <summary>How many bytes <see cref="Write"/> needs and <see cref="Read"/> consumes.</summary>
        public const int Length = 4;

        private const int BitsPerByte = 8;
        private const uint OffsetBasis = 2166136261u;
        private const uint Prime = 16777619u;

        /// <summary>Hashes a payload.</summary>
        /// <param name="payload">The bytes to hash. A null payload hashes to the empty value.</param>
        /// <returns>The hash of the payload.</returns>
        public static uint Compute(byte[] payload)
        {
            uint hash = OffsetBasis;

            if (payload == null)
                return hash;

            foreach (byte value in payload)
            {
                hash ^= value;
                hash *= Prime;
            }

            return hash;
        }

        /// <summary>
        /// Writes a checksum, least significant byte first. Written by hand rather than through
        /// <c>BitConverter</c> so the layout on disk does not depend on the machine that wrote it.
        /// </summary>
        /// <param name="checksum">The value to write.</param>
        /// <param name="target">The buffer to write into.</param>
        /// <param name="offset">Where in the buffer to start.</param>
        public static void Write(uint checksum, byte[] target, int offset)
        {
            for (int i = 0; i < Length; i++)
                target[offset + i] = (byte)(checksum >> i * BitsPerByte);
        }

        /// <summary>Reads a checksum written by <see cref="Write"/>.</summary>
        /// <param name="source">The buffer to read from.</param>
        /// <param name="offset">Where in the buffer to start.</param>
        /// <returns>The stored value.</returns>
        public static uint Read(byte[] source, int offset)
        {
            uint checksum = 0;

            for (int i = 0; i < Length; i++)
                checksum |= (uint)source[offset + i] << i * BitsPerByte;

            return checksum;
        }
    }
}