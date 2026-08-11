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
        /// <summary>Label of the add button.</summary>
        public const string AddLabel = "+";
        /// <summary>Width of the add and remove buttons at the end of a row.</summary>
        public const float ButtonWidth = 22f;

        /// <summary>Horizontal gap between two controls in a row.</summary>
        public const float Gap = 4f;

        /// <summary>Horizontal offset applied to every row below the header.</summary>
        public const float Indent = 14f;

        /// <summary>Label of the remove button.</summary>
        public const string RemoveLabel = "-";

        /// <summary>Vertical gap between two rows.</summary>
        public static float Spacing => EditorGUIUtility.standardVerticalSpacing;

        /// <summary>Height of a single control line.</summary>
        public static float Line => EditorGUIUtility.singleLineHeight;

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
        public static bool DrawHeader(Rect rect, SerializedProperty property, GUIContent label, int count)
        {
            Rect foldoutRect = new(rect.x, rect.y, rect.width - ButtonWidth - Gap, rect.height);
            GUIContent content = new($"{label.text} ({count})", label.tooltip);

            return EditorGUI.Foldout(foldoutRect, property.isExpanded, content, true);
        }

        /// <summary>Draws the add button at the right end of the header and returns true when clicked.</summary>
        public static bool DrawAddButton(Rect rect)
        {
            Rect buttonRect = new(rect.xMax - ButtonWidth, rect.y, ButtonWidth, Line);
            return GUI.Button(buttonRect, AddLabel, EditorStyles.miniButton);
        }

        /// <summary>Draws the remove button at the right end of a row and returns true when clicked.</summary>
        public static bool DrawRemoveButton(Rect rect)
        {
            Rect buttonRect = new(rect.xMax - ButtonWidth, rect.y, ButtonWidth, Line);
            return GUI.Button(buttonRect, RemoveLabel, EditorStyles.miniButton);
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
        public static void MarkDuplicate(Rect rect)
        {
            Color tint = new(1f, 0.3f, 0.3f, 0.12f);
            EditorGUI.DrawRect(new Rect(rect.x - Gap, rect.y - 1f, rect.width + Gap * 2f, rect.height + 2f), tint);
        }
    }
}