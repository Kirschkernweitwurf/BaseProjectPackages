using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Utility class for property drawers.
    /// Provides generic popup drawing for any UnityEngine.Object type.
    /// </summary>
    public static class PropertyDrawerUtility
    {
        /// <summary>
        /// Draws a popup for selecting an object reference of type T from a list of options.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object type.</typeparam>
        /// <param name="position">GUI position.</param>
        /// <param name="property">SerializedObject reference property.</param>
        /// <param name="label">Label for the popup.</param>
        /// <param name="options">List of objects to pick from.</param>
        /// <remarks>Includes a "None" option to clear the reference.</remarks>
        public static void DrawObjectPopup<T>(Rect position, SerializedProperty property, GUIContent label,
            List<T> options) where T : Object
        {
            if (options == null
                || property == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Build names array directly with "None" entry first (avoids intermediate list and LINQ garbage).
            string[] names = new string[options.Count + 1];
            names[0] = "None";
            for (int i = 0; i < options.Count; i++)
            {
                names[i + 1] = options[i] != null
                    ? options[i].name
                    : "<NULL>";
            }

            Object current = property.objectReferenceValue;

            // Determine current index with 1-based offset
            int index = 0;
            if (current is T typedCurrent)
            {
                int foundIndex = options.IndexOf(typedCurrent);
                if (foundIndex >= 0)
                    index = foundIndex + 1;
            }

            int newIndex = EditorGUI.Popup(position, label.text, index, names);

            // Only write back on an actual change so repaints don't touch the SerializedObject.
            if (newIndex == index)
                return;

            property.objectReferenceValue = newIndex == 0
                ? null
                : options[newIndex - 1];
        }
    }
}
