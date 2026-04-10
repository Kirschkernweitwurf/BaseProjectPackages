using System.Text;

namespace Utility.Types
{
    /// <summary>
    /// Central access point for global strings.
    /// </summary>
    public static class StringUtility
    {
        /// <summary>
        /// Returns a nicely formatted version of a variable name, by replacing underscores with spaces,
        /// inserting spaces before capital letters and capitalizing the first letter of each word.
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static string NicifyVariableName(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return string.Empty;

            variableName = variableName.Replace("_", " ");

            StringBuilder result = new();
            for (int i = 0; i < variableName.Length; i++)
            {
                char currentChar = variableName[i];

                if (i > 0 &&
                    char.IsUpper(currentChar) &&
                    char.IsLower(variableName[i - 1]))
                {
                    result.Append(' ');
                }

                result.Append(currentChar);
            }

            return result.ToString();
        }
    }
}