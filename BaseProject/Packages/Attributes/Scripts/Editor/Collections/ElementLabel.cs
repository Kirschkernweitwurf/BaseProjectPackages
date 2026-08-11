using UnityEditor;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Builds the text shown for one element of a list, which is also the text the search box matches
    /// against. A list of configs reads far better as a column of names than as a column of
    /// "Element 0", and a filter over indices would be useless.
    /// </summary>
    public static class ElementLabel
    {
        private const string IndexFormat = "Element {0}";
        private const string UnnamedFormat = "Element {0} (unnamed)";

        /// <summary>Returns the label for one element.</summary>
        /// <param name="element">The array element.</param>
        /// <param name="index">Its position in the array.</param>
        /// <param name="labelMember">Optional member on the element used as the label.</param>
        /// <returns>The text to show.</returns>
        public static string For(SerializedProperty element, int index, string labelMember)
        {
            SerializedProperty source = Resolve(element, labelMember);

            if (source == null)
                return string.Format(IndexFormat, index);

            string text = Read(source);

            return string.IsNullOrEmpty(text)
                ? string.Format(UnnamedFormat, index)
                : text;
        }

        // With no label member the element itself is the label, as long as it is a leaf. That is what
        // makes searching a list of strings or enums work without any extra configuration; a list of
        // structs has nothing readable to fall back to and keeps its index.
        private static SerializedProperty Resolve(SerializedProperty element, string labelMember)
        {
            if (!string.IsNullOrEmpty(labelMember))
                return element.FindPropertyRelative(labelMember);

            return element.propertyType == SerializedPropertyType.Generic
                ? null
                : element;
        }

        private static string Read(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.##");
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex >= 0 && property.enumValueIndex < property.enumDisplayNames.Length
                        ? property.enumDisplayNames[property.enumValueIndex]
                        : string.Empty;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null
                        ? string.Empty
                        : property.objectReferenceValue.name;
                default:
                    return string.Empty;
            }
        }
    }
}
