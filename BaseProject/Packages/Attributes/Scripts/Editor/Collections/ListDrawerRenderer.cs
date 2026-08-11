using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// Draws an array for <see cref="ListDrawerSettingsAttribute"/>: a foldout header with a count and an
    /// add button, an optional search box, optional paging, and one row per element with reorder and
    /// remove controls.
    /// </summary>
    internal static class ListDrawerRenderer
    {
        private const string EmptyMessage = "Empty.";
        private const string NextLabel = "\u25B6";
        private const string NoMatchMessage = "Nothing matches the filter.";
        private const string NoReorderTooltip = "Reordering is off while a filter or a page hides part of "
            + "the list, because the row above is not the element above.";
        private const string PageFormat = "{0} / {1}";
        private const float PageButtonWidth = 24f;
        private const string PreviousLabel = "\u25C0";
        private const float SearchWidth = 150f;

        // Reused between rows so the renderer allocates one list per repaint instead of one per row.
        private static readonly List<int> Visible = new();

        /// <summary>Draws the list.</summary>
        /// <param name="property">The array property.</param>
        /// <param name="label">The label and tooltip shown in the header.</param>
        /// <param name="settings">The settings that shape the list.</param>
        /// <param name="canResize">False when [ArraySize] fixes the element count.</param>
        public static void Draw(SerializedProperty property, GUIContent label,
            ListDrawerSettingsAttribute settings, bool canResize = true)
        {
            ListDrawerState state = ListDrawerState.For(property);

            if (ListDrawerState.IsFirstDraw(property))
                property.isExpanded = settings.DefaultExpanded;

            if (!DrawHeader(property, label, settings, state, canResize))
                return;

            Collect(property, settings, state);

            if (property.arraySize == 0)
            {
                EditorGUILayout.LabelField(EmptyMessage, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (Visible.Count == 0)
            {
                EditorGUILayout.LabelField(NoMatchMessage, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            int pageSize = ResolvePageSize(settings.PageSize, Visible.Count);
            int pageCount = PageCount(Visible.Count, settings.PageSize);
            state.Page = Mathf.Clamp(state.Page, 0, pageCount - 1);

            int first = state.Page * pageSize;
            int last = Mathf.Min(first + pageSize, Visible.Count);

            // Moving an element one step swaps it with its neighbour in the array. While a filter or a
            // page hides part of the list, that neighbour is not the row above or below, so the entry
            // would appear to jump somewhere at random. The arrows switch off rather than lie.
            bool canReorder = Visible.Count == property.arraySize && pageCount == 1;

            EditorGUI.indentLevel++;
            DrawRows(property, settings, first, last, canResize, canReorder);
            EditorGUI.indentLevel--;

            if (pageCount > 1)
                DrawPageBar(state, pageCount);
        }

        // Returns whether the body should be drawn.
        private static bool DrawHeader(SerializedProperty property, GUIContent label,
            ListDrawerSettingsAttribute settings, ListDrawerState state, bool canResize)
        {
            EditorGUILayout.BeginHorizontal();

            GUIContent header = new($"{label.text} ({property.arraySize})", label.tooltip);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, header, true);

            if (settings.Searchable && property.isExpanded)
            {
                GUILayout.FlexibleSpace();
                state.Search = EditorGUILayout.TextField(state.Search, EditorStyles.toolbarSearchField,
                    GUILayout.Width(SearchWidth));
            }

            if (canResize && !settings.HideAddButton
                && Button(CollectionGui.AddLabel, CollectionGui.ButtonWidth, true))
            {
                property.arraySize++;
                property.isExpanded = true;
            }

            EditorGUILayout.EndHorizontal();

            return property.isExpanded;
        }

        // Filtering happens on the label, which is the element's own value unless a label member says
        // otherwise. That is what makes a plain list of strings searchable without any configuration.
        private static void Collect(SerializedProperty property, ListDrawerSettingsAttribute settings,
            ListDrawerState state)
        {
            Visible.Clear();

            bool filtered = settings.Searchable && !string.IsNullOrEmpty(state.Search);

            for (int i = 0; i < property.arraySize; i++)
            {
                if (!filtered)
                {
                    Visible.Add(i);
                    continue;
                }

                string text = ElementLabel.For(property.GetArrayElementAtIndex(i), i, settings.LabelMember);

                if (text.IndexOf(state.Search, StringComparison.OrdinalIgnoreCase) >= 0)
                    Visible.Add(i);
            }
        }

        private static int ResolvePageSize(int pageSize, int total) => pageSize > 0
            ? pageSize
            : Mathf.Max(total, 1);

        private static int PageCount(int total, int pageSize)
        {
            if (pageSize <= 0)
                return 1;

            return Mathf.Max(1, Mathf.CeilToInt(total / (float)pageSize));
        }

        private static void DrawRows(SerializedProperty property, ListDrawerSettingsAttribute settings,
            int first, int last, bool canResize, bool canReorder)
        {
            int moveFrom = -1;
            int moveTo = -1;
            int removeAt = -1;

            for (int slot = first; slot < last; slot++)
            {
                int index = Visible[slot];
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                string label = ElementLabel.For(element, index, settings.LabelMember);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element, new GUIContent(label), true);

                if (!settings.HideReorderButtons)
                {
                    string tooltip = canReorder
                        ? string.Empty
                        : NoReorderTooltip;

                    if (Button(CollectionGui.MoveUpLabel, tooltip, canReorder && index > 0))
                    {
                        moveFrom = index;
                        moveTo = index - 1;
                    }

                    if (Button(CollectionGui.MoveDownLabel, tooltip,
                            canReorder && index < property.arraySize - 1))
                    {
                        moveFrom = index;
                        moveTo = index + 1;
                    }
                }

                if (canResize
                    && !settings.HideRemoveButton
                    && Button(CollectionGui.RemoveLabel, CollectionGui.SmallButtonWidth, true)
                    && CollectionGui.ConfirmRemoval(label, settings.ConfirmDelete))
                    removeAt = index;

                EditorGUILayout.EndHorizontal();

                // Rows sit flush against each other otherwise, which reads as one block of text rather
                // than a list of entries.
                GUILayout.Space(CollectionGui.RowGap);
            }

            // Applied after the loop so the array is never resized while it is being iterated.
            if (moveFrom >= 0)
                property.MoveArrayElement(moveFrom, moveTo);

            if (removeAt >= 0)
                CollectionGui.DeleteElement(property, removeAt);
        }

        // The arrows are drawn at line height rather than at the row height, so a multi-line element
        // does not stretch them into bars.
        private static bool Button(string label, float width, bool enabled)
        {
            Rect rect = GUILayoutUtility.GetRect(width, CollectionGui.Line, GUILayout.Width(width),
                GUILayout.Height(CollectionGui.Line));

            return CollectionGui.SmallButton(rect, label, enabled);
        }

        // A disabled control shows no tooltip, so the explanation for the switched-off arrows is carried
        // by the row they sit in rather than by the buttons themselves.
        private static bool Button(string label, string tooltip, bool enabled)
        {
            Rect rect = GUILayoutUtility.GetRect(CollectionGui.SmallButtonWidth, CollectionGui.Line,
                GUILayout.Width(CollectionGui.SmallButtonWidth), GUILayout.Height(CollectionGui.Line));

            if (!string.IsNullOrEmpty(tooltip))
                GUI.Label(rect, new GUIContent(string.Empty, tooltip));

            return CollectionGui.SmallButton(rect, label, enabled);
        }

        private static void DrawPageBar(ListDrawerState state, int pageCount)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (Button(PreviousLabel, PageButtonWidth, state.Page > 0))
                state.Page--;

            GUILayout.Label(string.Format(PageFormat, state.Page + 1, pageCount), EditorStyles.miniLabel);

            if (Button(NextLabel, PageButtonWidth, state.Page < pageCount - 1))
                state.Page++;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}