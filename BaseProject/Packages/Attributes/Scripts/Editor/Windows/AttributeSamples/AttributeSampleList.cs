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
        private const string CountFormat = "{0} of {1} attributes";
        private const string NoMatchMessage = "Nothing matches.";
        private const string StatePrefix = "SAMPLECATEGORY";

        /// <summary>Draws the list and selects into the window when a row is clicked.</summary>
        /// <param name="window">The window that owns the selection.</param>
        /// <param name="styles">The window's styles.</param>
        public static void Draw(AttributeSampleWindow window, AttributeSampleStyles styles)
        {
            AttributeSampleEntry[] entries = AttributeSampleRegistry.All();
            bool searching = !string.IsNullOrEmpty(window.Search);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            window.Search = EditorGUILayout.TextField(window.Search, EditorStyles.toolbarSearchField);
            EditorGUILayout.EndHorizontal();

            window.ListScroll = EditorGUILayout.BeginScrollView(window.ListScroll, styles.ListBackground);

            int shown = DrawCategories(window, styles, entries, searching);

            if (shown == 0)
                EditorGUILayout.LabelField(NoMatchMessage, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.LabelField(string.Format(CountFormat, shown, entries.Length), styles.Footer);
        }

        private static int DrawCategories(AttributeSampleWindow window, AttributeSampleStyles styles,
            AttributeSampleEntry[] entries, bool searching)
        {
            List<AttributeSampleEntry> matches = new();
            int shown = 0;
            int index = 0;

            while (index < entries.Length)
            {
                string category = entries[index].Category;

                matches.Clear();

                while (index < entries.Length && entries[index].Category == category)
                {
                    if (Matches(entries[index], window.Search))
                        matches.Add(entries[index]);

                    index++;
                }

                if (matches.Count == 0)
                    continue;

                shown += matches.Count;
                DrawCategory(window, styles, category, matches, searching);
            }

            return shown;
        }

        private static void DrawCategory(AttributeSampleWindow window, AttributeSampleStyles styles,
            string category, List<AttributeSampleEntry> matches, bool searching)
        {
            string key = StateKey.For(typeof(AttributeSampleWindow), StatePrefix, category);
            bool stored = EditorPrefs.GetBool(key, true);
            bool expanded = searching || stored;

            Rect header = EditorGUILayout.GetControlRect(false, AttributeSampleStyles.CategoryHeight);
            bool clicked = EditorGUI.Foldout(header, expanded, $"{category}  ({matches.Count})", true,
                styles.Category);

            // While searching the group is forced open, so a click there would only fight the search.
            if (!searching && clicked != stored)
                EditorPrefs.SetBool(key, clicked);

            if (!expanded)
                return;

            foreach (AttributeSampleEntry entry in matches)
                DrawEntry(window, styles, entry);
        }

        // The category is searched as well as the name, so typing a topic finds everything under it.
        private static bool Matches(in AttributeSampleEntry entry, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            return entry.Title.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                || entry.Category.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void DrawEntry(AttributeSampleWindow window, AttributeSampleStyles styles,
            in AttributeSampleEntry entry)
        {
            bool selected = window.IsSelected(entry);
            Rect rect = EditorGUILayout.GetControlRect(false, AttributeSampleStyles.EntryHeight);

            if (selected && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, GUI.skin.settings.selectionColor);

            if (GUI.Button(rect, entry.Title, selected
                    ? styles.SelectedEntry
                    : styles.Entry))
                window.Select(entry);
        }
    }
}