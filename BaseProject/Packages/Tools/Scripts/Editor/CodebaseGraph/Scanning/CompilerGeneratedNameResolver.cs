using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reads the owner out of a compiler generated name. Lambdas, local functions, iterators and async
    /// methods all compile into hidden types and methods whose names carry the original member in angle
    /// brackets, for example "&lt;Start&gt;b__0_1" or "&lt;Start&gt;d__5". Without this, half the graph
    /// would point at machinery instead of at the code that was actually written.
    /// </summary>
    public static class CompilerGeneratedNameResolver
    {
        private const string BackingFieldSuffix = "k__BackingField";
        private const char CloseBracket = '>';
        private const char OpenBracket = '<';

        /// <summary>True when the name was produced by the compiler rather than written by hand.</summary>
        /// <param name="name">Type or member name to test.</param>
        /// <returns>True for generated names.</returns>
        public static bool IsGeneratedName(string name)
            => !string.IsNullOrEmpty(name) && name.IndexOf(OpenBracket) >= 0;

        /// <summary>Extracts the name of the member a generated name belongs to.</summary>
        /// <param name="name">Generated type or member name.</param>
        /// <param name="ownerName">The owning member name when one is encoded.</param>
        /// <returns>True when an owner name could be read.</returns>
        public static bool TryGetOwnerName(string name, out string ownerName)
        {
            ownerName = null;
            if (string.IsNullOrEmpty(name))
                return false;

            int open = name.IndexOf(OpenBracket);
            if (open < 0)
                return false;

            int close = name.IndexOf(CloseBracket, open + 1);
            if (close <= open + 1)
                return false;

            ownerName = name.Substring(open + 1, close - open - 1);
            return !string.IsNullOrEmpty(ownerName);
        }

        /// <summary>Strips the backing field decoration so an auto property field maps to its property.</summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="propertyName">Name of the property the field backs.</param>
        /// <returns>True when the field is an auto property backing field.</returns>
        public static bool TryGetBackingPropertyName(string fieldName, out string propertyName)
        {
            propertyName = null;
            if (string.IsNullOrEmpty(fieldName))
                return false;

            if (!fieldName.EndsWith(BackingFieldSuffix, StringComparison.Ordinal))
                return false;

            return TryGetOwnerName(fieldName, out propertyName);
        }
    }
}
