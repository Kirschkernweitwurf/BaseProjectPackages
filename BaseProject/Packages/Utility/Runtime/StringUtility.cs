using System.Text;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Central access point for global strings.
    /// </summary>
    public static class StringUtility
    {
        // Headroom for the spaces inserted between words, so the builder does not have to grow.
        private const int BufferPadding = 8;
        private const uint HashOffsetBasis = 2166136261u;
        private const uint HashPrime = 16777619u;

        /// <summary>
        /// Returns a nicely formatted version of a variable name, by replacing underscores with spaces,
        /// inserting spaces before capital letters and capitalizing the first letter of each word.
        /// Leading and repeated underscores are word breaks with no word on one side, so they collapse
        /// instead of producing a blank: <c>_isDirty</c> reads as <c>Is Dirty</c>.
        /// </summary>
        /// <param name="variableName">The raw variable name to format.</param>
        /// <returns>The formatted display name.</returns>
        public static string NicifyVariableName(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return string.Empty;

            StringBuilder result = new(variableName.Length + BufferPadding);
            bool wordStart = true;

            for (int i = 0; i < variableName.Length; i++)
            {
                char currentChar = variableName[i];

                if (currentChar == '_')
                {
                    if (IsSeparatorNeeded(result))
                        result.Append(' ');

                    wordStart = true;
                    continue;
                }

                if (IsSeparatorNeeded(result)
                    && char.IsUpper(currentChar)
                    && char.IsLower(variableName[i - 1]))
                    result.Append(' ');

                result.Append(wordStart
                    ? char.ToUpperInvariant(currentChar)
                    : currentChar);

                wordStart = false;
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns an FNV-1a hash of a string.
        /// </summary>
        /// <param name="value">The string to hash. Null and empty both return the offset basis.</param>
        /// <returns>The hash value.</returns>
        /// <remarks>
        /// Unlike <see cref="string.GetHashCode"/> this stays identical across sessions, runtimes and Unity
        /// versions, so it is safe to derive colors, bucket indices or file names from it and expect the same
        /// result next time.
        /// </remarks>
        public static uint GetStableHash(string value)
        {
            uint hash = HashOffsetBasis;

            if (string.IsNullOrEmpty(value))
                return hash;

            foreach (char character in value)
            {
                hash ^= character;
                hash *= HashPrime;
            }

            return hash;
        }

        /// <summary>A word break only means something once a word is there and is not already closed.</summary>
        private static bool IsSeparatorNeeded(StringBuilder result)
            => result.Length > 0 && result[result.Length - 1] != ' ';
    }
}