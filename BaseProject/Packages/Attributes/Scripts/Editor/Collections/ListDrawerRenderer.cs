using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
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
        private const string NoDragMessage = "Dragging is off while a filter or a page hides part of the "
            + "list, because the row above is not the element above.";
        private const float PageButtonWidth = 24f;
        private const string PageFormat = "{0} / {1}";
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

            if (!DrawHeader(property, label, settings, state))
                return;

            Collect(property, settings, state);

            bool isFiltered = Visible.Count != property.arraySize;

            // An empty list still draws, because its footer is what the first row is added with. Only
            // the filtered fallback has nothing to show, since it has no footer of its own.
            if (property.arraySize == 0)
            {
                if (settings.Draggable)
                    DrawNativeRows(property, settings, canResize);
                else
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

            // Moving an element one step swaps it with its neighbor in the array. While a filter or a
            // page hides part of the list, that neighbor is not the row above or below, so the entry
            // would appear to jump somewhere at random. The arrows switch off rather than lie.
            bool canReorder = !isFiltered && pageCount == 1 && settings.Draggable;

            EditorGUI.indentLevel++;

            // Unity's own list owns its layout: it draws every element and cannot be told to skip any.
            // While nothing is hidden that is exactly what is wanted, and the rows behave the way every
            // other list in the editor does. As soon as a filter or a page hides part of the list, the
            // plain rows take over, because a dragged row would otherwise land somewhere the pointer
            // never went.
            if (canReorder)
                DrawNativeRows(property, settings, canResize);
            else
                DrawRows(property, settings, first, last, canResize);

            EditorGUI.indentLevel--;

            if (settings.Draggable && (isFiltered || pageCount > 1))
                EditorGUILayout.LabelField(NoDragMessage, EditorStyles.centeredGreyMiniLabel);

            if (pageCount > 1)
                DrawPageBar(state, pageCount);
        }

        // Returns whether the body should be drawn.
        private static bool DrawHeader(SerializedProperty property, GUIContent label,
            ListDrawerSettingsAttribute settings, ListDrawerState state)
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

            // No add button here. Adding lives in the footer under the rows, where Unity puts it, where
            // it keeps working on an empty list, and where it cannot land on top of the search box.
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

        // The height comes from the list itself and includes its footer, so nothing below is drawn on
        // top of the add and remove buttons. The list is fetched once rather than twice, because the
        // second call would reconfigure it between measuring and drawing.
        private static void DrawNativeRows(SerializedProperty property, ListDrawerSettingsAttribute settings,
            bool canResize)
        {
            ReorderableList list = ReorderableListCache.Get(property, settings, canResize);
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, list.GetHeight()));

            using (new NoIndentScope())
                list.DoList(rect);
        }

        private static void DrawRows(SerializedProperty property, ListDrawerSettingsAttribute settings,
            int first, int last, bool canResize)
        {
            int removeAt = -1;

            for (int slot = first; slot < last; slot++)
            {
                int index = Visible[slot];
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                string label = ElementLabel.For(element, index, settings.LabelMember);

                // The stripe is drawn behind the row, so the rect has to be reserved before the controls
                // fill it. The layout pass has no rect yet, which is what the vertical group is for: it
                // reports the height the row took, and the repaint pass paints into it.
                Rect row = EditorGUILayout.BeginVertical();

                if (settings.ShowAlternatingBackground)
                    CollectionGui.DrawStripe(Stretch(row), slot - first);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element, new GUIContent(label), true);

                if (canResize
                    && !settings.HideRemoveButton
                    && Button(CollectionGui.RemoveLabel, CollectionGui.SmallButtonWidth, true)
                    && CollectionGui.ConfirmRemoval(label, settings.ConfirmDelete))
                    removeAt = index;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                // Rows sit flush against each other otherwise, which reads as one block of text rather
                // than a list of entries.
                GUILayout.Space(CollectionGui.RowGap);
            }

            // Applied after the loop so the array is never resized while it is being iterated.
            if (removeAt >= 0)
                CollectionGui.DeleteElement(property, removeAt);
        }

        // The reserved rect stops at the indent, and a stripe that stops there reads as a box around the
        // row rather than as a band behind it. Widening it to the full inspector is what makes the
        // striping disappear into the background instead of drawing attention to itself.
        private static Rect Stretch(Rect row)
            => new(row.x - CollectionGui.RowGap, row.y - CollectionGui.RowGap * 0.5f,
                row.width + CollectionGui.RowGap * 2f, row.height + CollectionGui.RowGap);

        // The arrows are drawn at line height rather than at the row height, so a multi-line element
        // does not stretch them into bars.
        private static bool Button(string label, float width, bool enabled)
        {
            Rect rect = GUILayoutUtility.GetRect(width, CollectionGui.Line, GUILayout.Width(width),
                GUILayout.Height(CollectionGui.Line));

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