using System.Collections.Generic;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Tracks which properties have been drawn at least once this editor session, so a default expanded
    /// state can be applied exactly once. A SerializedProperty has no way to say whether its expanded
    /// flag was ever set, and forcing it every repaint would make the field impossible to fold away.
    /// </summary>
    internal static class FirstDraw
    {
        private const char KeySeparator = ':';

        private static readonly HashSet<string> Seen = new();

        /// <summary>Returns true the first time it is called for a given property, false afterwards.</summary>
        /// <param name="property">The property being drawn.</param>
        /// <returns>True on the first draw only.</returns>
        public static bool IsFirst(SerializedProperty property) => Seen.Add(KeyFor(property));

        private static string KeyFor(SerializedProperty property)
            => property.serializedObject.targetObject.GetInstanceID() + KeySeparator + property.propertyPath;
    }
}