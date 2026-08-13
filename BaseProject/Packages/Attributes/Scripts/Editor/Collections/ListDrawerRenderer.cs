using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Draws a <see cref="ListDrawerSettingsAttribute"/> collection: a foldout with an element count, an
    /// optional search box, and Unity's own reorderable list underneath.
    /// </summary>
    /// <remarks>
    /// There is one renderer and it is Unity's. Filtering is done by giving the rows that do not match a
    /// height of zero rather than by drawing a different list, so a filtered list is the same control
    /// with fewer rows visible instead of a second implementation that has to be kept looking identical.
    /// </remarks>
    internal static class ListDrawerRenderer
    {
        private const string CountFormat = "{0} ({1})";
        private const string FilteredCountFormat = "{0} ({1} of {2})";
        private const float SearchWidth = 150f;

        // Reused between draws so filtering does not allocate a set per repaint.
        private static readonly HashSet<int> Hidden = new();

        /// <summary>Draws the collection.</summary>
        /// <param name="property">The array to draw.</param>
        /// <param name="label">The label and tooltip of the field.</param>
        /// <param name="settings">The settings that shape the list.</param>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        public static void Draw(SerializedProperty property, GUIContent label,
            ListDrawerSettingsAttribute settings, bool canResize = true)
        {
            ListDrawerState state = ListDrawerState.For(property);

            if (!DrawHeader(property, label, settings, state))
                return;

            CollectHidden(property, settings, state);

            // No extra indent. The table and a plain array start at the field's own left edge, and a list
            // that started one step further right read as belonging to something else.
            ReorderableList list = ReorderableListCache.Get(property, settings, canResize, Hidden);
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, list.GetHeight()));

            using (new NoIndentScope())
                list.DoList(rect);
        }

        // The count in the header says how many rows the filter left, because a list that reads (40)
        // while showing three is confusing in exactly the moment the filter is meant to help.
        private static bool DrawHeader(SerializedProperty property, GUIContent label,
            ListDrawerSettingsAttribute settings, ListDrawerState state)
        {
            EditorGUILayout.BeginHorizontal();

            string text = Hidden.Count == 0
                ? string.Format(CountFormat, label.text, property.arraySize)
                : string.Format(FilteredCountFormat, label.text, property.arraySize - Hidden.Count,
                    property.arraySize);

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded,
                new GUIContent(text, label.tooltip), true);

            if (settings.Searchable && property.isExpanded)
            {
                GUILayout.FlexibleSpace();

                state.Search = EditorGUILayout.TextField(state.Search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(SearchWidth));
            }

            EditorGUILayout.EndHorizontal();

            return property.isExpanded;
        }

        private static void CollectHidden(SerializedProperty property,
            ListDrawerSettingsAttribute settings, ListDrawerState state)
        {
            Hidden.Clear();

            if (!settings.Searchable || string.IsNullOrEmpty(state.Search))
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                string label = ElementLabel.For(property.GetArrayElementAtIndex(i), i,
                    settings.LabelMember);

                if (label.IndexOf(state.Search, System.StringComparison.OrdinalIgnoreCase) < 0)
                    Hidden.Add(i);
            }
        }
    }
}