using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
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
    /// </remarks>
    internal static class AttributeSampleList
    {
        private const string ClearLabel = "\u00D7";
        private const string ClearTooltip = "Clear the search";
        private const float ClearWidth = 22f;
        private const float SearchPadding = 4f;
        private const string CountFormat = "{0} of {1} attributes";
        private const string NoMatchMessage = "Nothing matches.";
        private const string StatePrefix = "SAMPLECATEGORY";

        // Reused between draws so filtering does not allocate a list per repaint.
        private static readonly List<AttributeSampleEntry> Matches = new();

        /// <summary>Draws the list and selects into the window when a row is clicked.</summary>
        /// <param name="window">The window that owns the selection.</param>
        /// <param name="styles">The window's styles.</param>
        public static void Draw(AttributeSampleWindow window, AttributeSampleStyles styles)
        {
            AttributeSampleEntry[] entries = AttributeSampleRegistry.All();
            bool searching = !string.IsNullOrEmpty(window.Search);

            AttributeSampleRegistry.Visible.Clear();

            DrawSearchBar(window);

            window.ListScroll = EditorGUILayout.BeginScrollView(window.ListScroll, styles.ListBackground);

            int shown = DrawCategories(window, styles, entries, searching);

            if (shown == 0)
                EditorGUILayout.LabelField(NoMatchMessage, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField(string.Format(CountFormat, shown, entries.Length), styles.Footer);
        }

        private static void DrawSearchBar(AttributeSampleWindow window)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // The search field otherwise sits hard against the window edge, where every other toolbar in
            // the editor leaves a margin.
            GUILayout.Space(SearchPadding);

            window.Search = EditorGUILayout.TextField(window.Search, EditorStyles.toolbarSearchField);

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(window.Search)))
            {
                if (GUILayout.Button(new GUIContent(ClearLabel, ClearTooltip),
                        EditorStyles.toolbarButton, GUILayout.Width(ClearWidth)))
                {
                    window.Search = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static int DrawCategories(AttributeSampleWindow window, AttributeSampleStyles styles,
            AttributeSampleEntry[] entries, bool searching)
        {
            int shown = 0;
            int index = 0;

            while (index < entries.Length)
            {
                string category = entries[index].Category;

                Matches.Clear();

                while (index < entries.Length && entries[index].Category == category)
                {
                    if (IsMatch(entries[index], window.Search))
                        Matches.Add(entries[index]);

                    index++;
                }

                if (Matches.Count == 0)
                    continue;

                shown += Matches.Count;
                DrawCategory(window, styles, category, searching);
            }

            return shown;
        }

        private static void DrawCategory(AttributeSampleWindow window, AttributeSampleStyles styles,
            string category, bool searching)
        {
            string key = StateKey.For(typeof(AttributeSampleWindow), StatePrefix, category);
            bool stored = EditorPrefs.GetBool(key, true);
            bool expanded = searching || stored;

            Rect header = EditorGUILayout.GetControlRect(false, AttributeSampleStyles.CategoryHeight);
            bool clicked = EditorGUI.Foldout(header, expanded, $"{category}  ({Matches.Count})", true,
                styles.Category);

            // While searching the group is forced open, so a click there would only fight the search.
            if (!searching && clicked != stored)
                EditorPrefs.SetBool(key, clicked);

            if (!expanded)
                return;

            for (int i = 0; i < Matches.Count; i++)
            {
                AttributeSampleRegistry.Visible.Add(Matches[i]);
                DrawEntry(window, styles, Matches[i], i);
            }
        }

        // The description is searched as well as the name and the category, so a word from the
        // explanation finds the attribute even when its name is not what you would have guessed.
        private static bool IsMatch(in AttributeSampleEntry entry, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            return Contains(entry.Title, search)
                || Contains(entry.Category, search)
                || Contains(entry.Description, search);
        }

        private static bool Contains(string text, string search) => !string.IsNullOrEmpty(text)
            && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private static void DrawEntry(AttributeSampleWindow window, AttributeSampleStyles styles,
            in AttributeSampleEntry entry, int row)
        {
            bool selected = window.IsSelected(entry);
            Rect rect = EditorGUILayout.GetControlRect(false, AttributeSampleStyles.EntryHeight);

            if (Event.current.type == EventType.Repaint)
                DrawRowBackground(styles, rect, selected, row);

            if (GUI.Button(rect, new GUIContent(entry.Title, entry.Title), selected
                    ? styles.SelectedEntry
                    : styles.Entry))
                window.Select(entry);

            // A row the pointer is over is repainted so the highlight follows it, which a window only
            // repainting on interaction would otherwise not do.
            if (rect.Contains(Event.current.mousePosition))
                window.Repaint();
        }

        private static void DrawRowBackground(AttributeSampleStyles styles, Rect rect, bool selected, int row)
        {
            if (selected)
            {
                EditorGUI.DrawRect(rect, GUI.skin.settings.selectionColor);
                return;
            }

            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, styles.Hover);
                return;
            }

            if (row % 2 != 0)
                EditorGUI.DrawRect(rect, styles.Stripe);
        }
    }
}