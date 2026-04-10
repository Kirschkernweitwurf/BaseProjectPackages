namespace Utility.Types
{
    /// <summary>
    /// This class contains extension methods for the Type class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Converts a boolean value to an integer.
        /// </summary>
        /// <param name="value"> The boolean value to convert.</param>
        /// <returns>value ? 1 : 0</returns>
        public static int ToInt(this bool value) => value ? 1 : 0;

        /// <summary>
        /// Converts an integer value to a boolean.
        /// </summary>
        /// <param name="value">The integer value to convert.</param>
        /// <returns>value != 0</returns>
        public static bool ToBool(this int value) => value != 0;
    }
}