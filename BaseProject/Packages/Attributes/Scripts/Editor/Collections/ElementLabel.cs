using UnityEditor;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Builds the text shown for one element of a list, which is also the text the search box matches
    /// against. A list of configs reads far better as a column of names than as a column of
    /// "Element 0", and a filter over indices would be useless.
    /// </summary>
    /// <remarks>
    /// The member to read is found rather than configured. Naming it was a setting for something Unity
    /// now does by itself: its own list labels a row after the first string on the element, so a setting
    /// that usually named that same field was asking for work to arrive at the default. The first string
    /// child is used, and a list wanting some other field is a list whose first field is in the wrong
    /// place.
    /// </remarks>
    internal static class ElementLabel
    {
        private const string IndexFormat = "Element {0}";
        private const string UnnamedFormat = "Element {0} (unnamed)";

        /// <summary>Returns the label for one element.</summary>
        /// <param name="element">The array element.</param>
        /// <param name="index">Its position in the array.</param>
        /// <returns>The text to show.</returns>
        internal static string For(SerializedProperty element, int index)
        {
            SerializedProperty source = Resolve(element);

            if (source == null)
                return string.Format(IndexFormat, index);

            string text = Read(source);

            return string.IsNullOrEmpty(text)
                ? string.Format(UnnamedFormat, index)
                : text;
        }

        // A leaf element is its own label, which is what makes searching a list of strings or enums work
        // with no configuration at all. A struct has no single value to show, so its first string child
        // is used and anything without one keeps its index.
        private static SerializedProperty Resolve(SerializedProperty element)
        {
            if (element.propertyType != SerializedPropertyType.Generic)
                return element;

            SerializedProperty child = element.Copy();
            SerializedProperty end = element.GetEndProperty();

            if (!child.NextVisible(true))
                return null;

            while (!SerializedProperty.EqualContents(child, end))
            {
                if (child.propertyType == SerializedPropertyType.String)
                    return child;

                if (!child.NextVisible(false))
                    break;
            }

            return null;
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