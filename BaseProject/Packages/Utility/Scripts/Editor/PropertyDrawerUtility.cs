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
        private const string MissingOptionLabel = "<NULL>";
        private const int NoneOptionIndex = 0;
        private const string NoneOptionLabel = "None";

        /// <summary>
        /// Draws a popup for selecting an object reference of type <typeparamref name="T"/> from a list of options.
        /// </summary>
        /// <typeparam name="T">Any UnityEngine.Object type.</typeparam>
        /// <param name="position">GUI position.</param>
        /// <param name="property">The object reference property to write to.</param>
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

            string[] names = BuildNames(options);
            int index = GetCurrentIndex(property, options);
            int newIndex = LabeledField.Popup(position, label, index, names);

            // Only write back on an actual change so repaints do not touch the SerializedObject.
            if (newIndex == index)
                return;

            property.objectReferenceValue = newIndex == NoneOptionIndex
                ? null
                : options[newIndex - 1];
        }

        // Builds the names array directly with the "None" entry first, avoiding an intermediate list and LINQ garbage.
        private static string[] BuildNames<T>(List<T> options) where T : Object
        {
            string[] names = new string[options.Count + 1];
            names[NoneOptionIndex] = NoneOptionLabel;

            for (int i = 0; i < options.Count; i++)
            {
                names[i + 1] = options[i] == null
                    ? MissingOptionLabel
                    : options[i].name;
            }

            return names;
        }

        // The popup index is offset by one, because the "None" entry occupies index zero.
        private static int GetCurrentIndex<T>(SerializedProperty property, List<T> options) where T : Object
        {
            if (property.objectReferenceValue is not T current)
                return NoneOptionIndex;

            int foundIndex = options.IndexOf(current);

            return foundIndex < 0
                ? NoneOptionIndex
                : foundIndex + 1;
        }
    }
}