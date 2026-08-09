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
        public static string Display<T>() where T : Attribute => Display(typeof(T));

        /// <summary>
        /// Returns the attribute name without the "Attribute" suffix. The non-generic form exists for
        /// callers that only hold a <see cref="Type"/>, such as table-driven diagnostics.
        /// </summary>
        /// <param name="attributeType">The attribute type to name.</param>
        /// <returns>The trimmed display name.</returns>
        public static string Display(Type attributeType)
        {
            string name = attributeType.Name;
            return name.EndsWith(Suffix)
                ? name[..^Suffix.Length]
                : name;
        }

        /// <summary>Builds a usage hint, for example "Use [Tag] with a string.".</summary>
        public static string Usage<T>(string requirement) where T : Attribute
            => Usage(typeof(T), requirement);

        /// <summary>
        /// Builds a usage hint from an attribute type. The non-generic form exists for callers that only
        /// hold a <see cref="Type"/>, such as table-driven diagnostics.
        /// </summary>
        /// <param name="attributeType">The attribute type to name.</param>
        /// <param name="requirement">What the attribute needs, for example "a string".</param>
        /// <returns>The usage hint.</returns>
        public static string Usage(Type attributeType, string requirement)
            => $"Use [{Display(attributeType)}] with {requirement}.";
    }
}