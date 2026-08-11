using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reads the owner out of a compiler generated name. Lambdas, local functions, iterators and async
    /// methods all compile into hidden types and methods whose names carry the original member in angle
    /// brackets, for example "&lt;Start&gt;b__0_1" or "&lt;Start&gt;d__5". Without this, half the graph
    /// would point at machinery instead of at the code that was actually written.
    /// </summary>
    internal static class CompilerGeneratedNameResolver
    {
        private const string AddPrefix = "add_";
        private const string BackingFieldSuffix = "k__BackingField";
        private const char CloseBracket = '>';
        private const string GetPrefix = "get_";
        private const int MaxUnwrapDepth = 4;
        private const char OpenBracket = '<';
        private const string RemovePrefix = "remove_";
        private const string SetPrefix = "set_";

        /// <summary>The four accessor prefixes the compiler puts in front of a property or event.</summary>
        private static readonly string[] AccessorPrefixes =
        {
            GetPrefix,
            SetPrefix,
            AddPrefix,
            RemovePrefix
        };

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

            // The brackets nest. An iterator inside a getter is named <<get_Order>b__1_0>d, so taking
            // the first closing bracket would cut the name in half and leave something meaningless.
            int close = FindMatchingBracket(name, open);
            if (close <= open + 1)
                return false;

            ownerName = name.Substring(open + 1, close - open - 1);
            return !string.IsNullOrEmpty(ownerName);
        }

        /// <summary>
        /// Reads the written member a generated name finally belongs to. One unwrap is not always
        /// enough: an async lambda or an iterator inside a property getter nests its machinery, so the
        /// first unwrap yields another generated name rather than the accessor.
        /// </summary>
        /// <param name="name">Generated member name.</param>
        /// <param name="ownerName">The written member it belongs to.</param>
        /// <returns>True when an owner could be read.</returns>
        public static bool TryResolveOwnerName(string name, out string ownerName)
        {
            ownerName = null;
            string current = name;

            for (int depth = 0; depth < MaxUnwrapDepth; depth++)
            {
                if (!TryGetOwnerName(current, out string next))
                    break;

                current = next;
                ownerName = next;

                if (!IsGeneratedName(current))
                    break;
            }

            return !string.IsNullOrEmpty(ownerName);
        }

        /// <summary>
        /// Strips an accessor prefix, so get_Order becomes Order. A lambda written inside a property
        /// getter encodes its owner as the accessor, but only the property itself is ever registered as
        /// a member, so without this the owner is never found and every call the lambda makes is lost.
        /// </summary>
        /// <param name="name">Member name, which may name an accessor.</param>
        /// <param name="ownerName">The property or event the accessor belongs to.</param>
        /// <returns>True when the name was an accessor.</returns>
        public static bool TryGetAccessorOwner(string name, out string ownerName)
        {
            ownerName = null;
            if (string.IsNullOrEmpty(name))
                return false;

            foreach (string prefix in AccessorPrefixes)
            {
                if (!name.StartsWith(prefix, StringComparison.Ordinal) || name.Length <= prefix.Length)
                    continue;

                ownerName = name[prefix.Length..];
                return true;
            }

            return false;
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

        private static int FindMatchingBracket(string name, int open)
        {
            int depth = 0;

            for (int index = open; index < name.Length; index++)
            {
                if (name[index] == OpenBracket)
                    depth++;
                else if (name[index] == CloseBracket)
                    depth--;

                if (depth == 0)
                    return index;
            }

            return -1;
        }
    }
}