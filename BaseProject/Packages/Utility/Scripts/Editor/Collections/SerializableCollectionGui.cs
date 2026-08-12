using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Collections
{
    /// <summary>
    /// Shared layout metrics and helpers for the serialized dictionary and set drawers, so both look
    /// and behave the same. Rows are drawn from explicit rects, which is why indentation is handled
    /// here instead of through <c>EditorGUI.indentLevel</c>.
    /// </summary>
    public static class SerializableCollectionGui
    {
        /// <summary>Horizontal gap between two controls in a row.</summary>
        public const float Gap = 4f;

        // How far the duplicate tint reaches past the row, so it reads as a band behind the whole entry
        // rather than as a box drawn inside it.
        private const float RowBleed = 1f;

        public static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        /// <summary>Height of a single control line.</summary>
        public static float Line => EditorGUIUtility.singleLineHeight;

        /// <summary>Vertical gap between two rows.</summary>
        private static readonly Color DuplicateTint = new(1f, 0.3f, 0.3f, 0.12f);

        /// <summary>
        /// Returns a value that identifies the property for duplicate detection, or null when the
        /// property type cannot be compared reliably. Unsupported types simply skip the check.
        /// </summary>
        public static string Identity(SerializedProperty property)
        {
            if (property == null)
                return null;

            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Character:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.doubleValue.ToString("R");
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceInstanceIDValue.ToString();
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("R");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("R");
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString("R");
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the indices of every element after the first that shares an identity with an earlier
        /// one. An empty result means the collection is free of duplicates or cannot be checked.
        /// </summary>
        public static HashSet<int> FindDuplicates(IReadOnlyList<SerializedProperty> values)
        {
            HashSet<int> duplicates = new();
            Dictionary<string, int> seen = new();

            for (int i = 0; i < values.Count; i++)
            {
                string identity = Identity(values[i]);
                if (identity == null)
                    continue;

                if (seen.ContainsKey(identity))
                    duplicates.Add(i);
                else
                    seen[identity] = i;
            }

            return duplicates;
        }

        /// <summary>Draws a foldout header with an element count and returns the new expanded state.</summary>
        /// <remarks>
        /// The header takes the whole row. Adding and removing moved to the list's own footer, so there
        /// is no longer a button up here to leave room for.
        /// </remarks>
        /// <param name="rect">The row the header occupies.</param>
        /// <param name="property">The collection being drawn.</param>
        /// <param name="label">The label and tooltip of the field.</param>
        /// <param name="count">How many elements it holds.</param>
        /// <returns>True while the collection is open.</returns>
        public static bool DrawHeader(Rect rect, SerializedProperty property, GUIContent label, int count)
        {
            GUIContent content = new($"{label.text} ({count})", label.tooltip);

            return EditorGUI.Foldout(rect, property.isExpanded, content, true);
        }

        /// <summary>
        /// Removes an element from a serialized array. Object reference elements are cleared first,
        /// because Unity's delete only nulls them on the first call and removes them on the second.
        /// </summary>
        /// <param name="array">The serialized array to remove from.</param>
        /// <param name="index">Index of the element to remove.</param>
        public static void DeleteElement(SerializedProperty array, int index)
        {
            SerializedProperty element = array.GetArrayElementAtIndex(index);

            if (element.propertyType == SerializedPropertyType.ObjectReference
                && element.objectReferenceValue != null)
                element.objectReferenceValue = null;

            int size = array.arraySize;
            array.DeleteArrayElementAtIndex(index);

            if (array.arraySize == size)
                array.DeleteArrayElementAtIndex(index);
        }

        /// <summary>Tints the given row to mark it as a duplicate.</summary>
        /// <param name="rect">The row to tint.</param>
        public static void MarkDuplicate(Rect rect)
        {
            Rect tinted = new(rect.x - Gap, rect.y - RowBleed, rect.width + Gap * 2f,
                rect.height + RowBleed * 2f);

            EditorGUI.DrawRect(tinted, DuplicateTint);
        }
    }
}