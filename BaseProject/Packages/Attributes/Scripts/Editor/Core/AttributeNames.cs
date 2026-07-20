using System;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Builds display names and usage messages for attributes from their type, so UI strings survive
    /// renames. The "Attribute" suffix is trimmed, so <see cref="TagAttribute"/> is shown as [Tag].
    /// </summary>
    public static class AttributeNames
    {
        private const string Suffix = "Attribute";

        /// <summary>Returns the attribute name without the "Attribute" suffix.</summary>
        public static string Display<T>() where T : Attribute
        {
            string name = typeof(T).Name;
            return name.EndsWith(Suffix)
                ? name[..^Suffix.Length]
                : name;
        }

        /// <summary>Builds a usage hint, for example "Use [Tag] with a string.".</summary>
        public static string Usage<T>(string requirement) where T : Attribute
            => $"Use [{Display<T>()}] with {requirement}.";
    }
}