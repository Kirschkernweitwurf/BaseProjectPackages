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

        /// <summary>
        /// Returns a nicely formatted version of a variable name, by replacing underscores with spaces,
        /// inserting spaces before capital letters and capitalizing the first letter of each word.
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
                    result.Append(' ');
                    wordStart = true;
                    continue;
                }

                if (i > 0
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
    }
}