using System;
using System.Collections.Generic;
using Base.AttributePackage.Samples;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Reference
{
    /// <summary>
    /// Draws the attribute list: a search box, then the attributes under collapsible category headers.
    /// </summary>
    /// <remarks>
    /// A plain list rather than a tree view. There are two levels and no reordering, which is less than
    /// a tree view is for, and a list is a fraction of the code with none of the version differences.
    /// <para>
    /// A category opens itself while a search is active, because a collapsed group that hides a match is
    /// worse than no grouping at all.
    /// </para>
    /// <para>
    /// The rows are remembered as they are drawn, because the keyboard walks what is on screen and only
    /// the draw knows which categories are open and what the search matched.
    /// </para>
    /// </remarks>
    internal sealed class AttributeReferenceList
    {
        private const string CancelStyleName = "ToolbarSearchCancelButton";
        private const string CancelStyleNameLegacy = "ToolbarSeachCancelButton";
        private const string ClearTooltip = "Clear the search";
        private const float ClearWidth = 16f;
        private const string CountFormat = "{0} of {1} attributes";
        private const float CountWidth = 34f;
        private const string NoMatchMessage = "Nothing matches.";
        private const float SearchPadding = 4f;
        private const string StatePrefix = "ReferenceCategory";

        private static readonly GUIContent ClearContent = new(string.Empty, ClearTooltip);

        private static GUIStyle _cancelStyle;

        private readonly List<AttributeSampleEntry> _matches = new();
        private readonly List<AttributeSampleRow> _rows = new();

        // The reused row content keeps the list from allocating one per row per repaint.
        private readonly GUIContent _rowContent = new();

        private int _stripe;

        /// <summary>The rows as they were last drawn, headers included, in the order they appear.</summary>
        internal IReadOnlyList<AttributeSampleRow> Rows => _rows;

        /// <summary>Whether the named category is open.</summary>
        /// <param name="category">The category to test.</param>
        /// <returns>True while it is expanded.</returns>
        internal static bool IsExpanded(string category) => EditorPrefs.GetBool(KeyFor(category), true);

        /// <summary>Opens or closes the named category.</summary>
        /// <param name="category">The category to change.</param>
        /// <param name="expanded">The new state.</param>
        internal static void SetExpanded(string category, bool expanded)
            => EditorPrefs.SetBool(KeyFor(category), expanded);

        /// <summary>Draws the list and selects into the pane when a row is clicked.</summary>
        /// <param name="pane">The pane that owns the selection.</param>
        /// <param name="styles">The window styles.</param>
        internal void Draw(AttributeReferencePane pane, AttributeExplorerStyles styles)
        {
            AttributeSampleEntry[] entries = AttributeSampleRegistry.All();
            bool searching = !string.IsNullOrEmpty(pane.Search);

            _rows.Clear();
            _stripe = 0;

            DrawSearchBar(pane);

            pane.ListScroll = EditorGUILayout.BeginScrollView(pane.ListScroll, styles.ListBackground);

            int shown = DrawCategories(pane, styles, entries, searching);

            if (shown == 0)
                EditorGUILayout.LabelField(NoMatchMessage, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField(string.Format(CountFormat, shown, entries.Length), styles.Footer);
        }

        // Unity spells this style differently across versions and has misspelled it in the past, so both
        // names are tried and a missing one falls back rather than throwing.
        private static GUIStyle CancelStyle()
        {
            if (_cancelStyle != null)
                return _cancelStyle;

            _cancelStyle = GUI.skin.FindStyle(CancelStyleName)
                ?? GUI.skin.FindStyle(CancelStyleNameLegacy)
                ?? EditorStyles.toolbarButton;

            return _cancelStyle;
        }

        private static string KeyFor(string category)
            => StateKey.For(typeof(AttributeReferenceList), StatePrefix, category);

        private static void DrawSearchBar(AttributeReferencePane pane)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // The search field otherwise sits hard against the window edge, where every other toolbar in
            // the editor leaves a margin.
            GUILayout.Space(SearchPadding);

            pane.Search = EditorGUILayout.TextField(pane.Search, EditorStyles.toolbarSearchField);

            // Only shown while there is something to clear, which is what the editor's own search fields
            // do. A permanently visible button next to an empty field is a control with nothing to do.
            if (!string.IsNullOrEmpty(pane.Search)
                && GUILayout.Button(ClearContent, CancelStyle(), GUILayout.Width(ClearWidth)))
            {
                pane.Search = string.Empty;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        // The description is searched as well as the name and the category, so a word from the
        // explanation finds the attribute even when its name is not what you would have guessed.
        private static bool IsMatch(in AttributeSampleEntry entry, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            return Contains(entry.Title, search)
                || Contains(entry.CategoryName, search)
                || Contains(entry.Description, search);
        }

        private static bool Contains(string text, string search) => !string.IsNullOrEmpty(text)
            && text.Contains(search, StringComparison.OrdinalIgnoreCase);

        private int DrawCategories(AttributeReferencePane pane, AttributeExplorerStyles styles,
            AttributeSampleEntry[] entries, bool searching)
        {
            int shown = 0;
            int index = 0;

            while (index < entries.Length)
            {
                EAttributeCategory category = entries[index].Category;
                string name = entries[index].CategoryName;

                _matches.Clear();

                while (index < entries.Length && entries[index].Category == category)
                {
                    if (IsMatch(entries[index], pane.Search))
                        _matches.Add(entries[index]);

                    index++;
                }

                if (_matches.Count == 0)
                    continue;

                shown += _matches.Count;
                DrawCategory(pane, styles, name, searching);
            }

            return shown;
        }

        private void DrawCategory(AttributeReferencePane pane, AttributeExplorerStyles styles, string category,
            bool searching)
        {
            string key = KeyFor(category);
            bool stored = EditorPrefs.GetBool(key, true);
            bool expanded = searching || stored;

            _rows.Add(new AttributeSampleRow(category));

            Rect header = EditorGUILayout.GetControlRect(false, AttributeExplorerStyles.CategoryHeight);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(header, styles.CategoryBand);

                if (pane.IsCategorySelected(category))
                    EditorGUI.DrawRect(new Rect(header.x, header.y,
                        AttributeExplorerStyles.SelectionBarWidth, header.height), styles.Selection);
            }

            float inset = AttributeExplorerStyles.SelectionBarWidth + AttributeExplorerStyles.SelectionBarGap;

            Rect count = new(header.xMax - CountWidth, header.y, CountWidth, header.height);
            Rect label = new(header.x + inset, header.y, header.width - CountWidth - inset, header.height);

            // Clicking a header opens its page as well as folding it, since a reader who does not know
            // the name of the attribute they want starts from the category rather than from the list.
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0
                && label.Contains(Event.current.mousePosition))
                pane.SelectCategory(category);

            _rowContent.text = category;
            bool clicked = EditorGUI.Foldout(label, expanded, _rowContent, true, styles.Category);

            GUI.Label(count, _matches.Count.ToString(), styles.Footer);

            // While searching the group is forced open, so a click there would only fight the search.
            if (!searching && clicked != stored)
                EditorPrefs.SetBool(key, clicked);

            if (!expanded)
                return;

            foreach (AttributeSampleEntry entry in _matches)
            {
                _rows.Add(new AttributeSampleRow(entry));
                DrawEntry(pane, styles, entry);
            }
        }

        private void DrawEntry(AttributeReferencePane pane, AttributeExplorerStyles styles,
            in AttributeSampleEntry entry)
        {
            bool selected = pane.IsSelected(entry);
            Rect rect = EditorGUILayout.GetControlRect(false, AttributeExplorerStyles.EntryHeight);

            // Striping runs across the whole list rather than restarting under every header, so two
            // categories meeting do not put two tinted rows next to each other.
            _stripe++;

            if (Event.current.type == EventType.Repaint)
                DrawRowBackground(styles, rect, selected);

            _rowContent.text = entry.Title;

            if (GUI.Button(rect, _rowContent, selected
                    ? styles.SelectedEntry
                    : styles.Entry))
                pane.Select(entry);
        }

        private void DrawRowBackground(AttributeExplorerStyles styles, Rect rect, bool selected)
        {
            if (selected)
            {
                EditorGUI.DrawRect(rect, styles.SelectionFill);
                EditorGUI.DrawRect(
                    new Rect(rect.x, rect.y, AttributeExplorerStyles.SelectionBarWidth, rect.height),
                    styles.Selection);

                return;
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, styles.Hover);
                return;
            }

            if (_stripe % 2 == 0)
                EditorGUI.DrawRect(rect, styles.Stripe);
        }
    }
}