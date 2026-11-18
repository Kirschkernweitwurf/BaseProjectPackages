using System;
using System.Collections.Generic;
using System.Linq;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Draws one card per text style: its size in every language, green when every text fits and red when one does
    /// not and the largest size that still fits. A style that does not fit gets buttons to shrink it in one language
    /// or in all of them. A click hands the sizes back to the window, which sets them and checks again.
    /// </summary>
    internal sealed class TextFitSizesView
    {
        private const string AllFitTooltip = "The largest size that fits in every language, so this kind of box "
            + "looks the same no matter the language.";
        private const string ApplyAllFormat = "Use {0} Everywhere";
        private const string ApplyFormat = "Use {0}";
        private const string ApplyTooltipFormat = "Every {0} text in {1} still fits at {2}.";
        private const string FitsFormat = "fits up to {0}";
        private const string FoldedRangeFormat = "Size {0} to {1} by language  ·  ";
        private const string FoldedSizeFormat = "Size {0} in every language  ·  ";
        private const string HideLabel = "Hide Languages";
        private const string Hint = "The size of every text style per language and the largest that still fits. "
            + "A style that does not fit gets a button to shrink it; growing one is a design choice. Undo works.";
        private const string NoFitText = "nothing fits down to the readable minimum";
        private const string NoFitTooltipFormat = "Some {0} text in {1} does not fit even at the readable minimum. "
            + "Shorten the text first.";
        private const string ShowLabel = "Show Languages";
        private const string ShowTooltip = "Every language fits, so the style shows as one line. Lists each one.";
        private const float SizeWidth = 34f;
        private const string SizeTooltipFormat = "{0} in {1} is size {2} now.";

        private readonly HashSet<string> _expanded = new();
        private readonly EditorWindowStyles _styles;

        /// <summary>Creates the view, drawing with <paramref name="styles"/>.</summary>
        internal TextFitSizesView(EditorWindowStyles styles) => _styles = styles;

        /// <summary>
        /// Draws every style that matches <paramref name="search"/>, the ones that need a new size first. Returns
        /// the rows and the size to set when a button was clicked or null.
        /// </summary>
        /// <param name="suggestions">The suggestions per style.</param>
        /// <param name="locale">The language code to show or null for every language.</param>
        /// <param name="search">Text to look for in the style names or empty to show every style.</param>
        internal (List<SizeSuggestion> Rows, float Size)? Draw(Dictionary<string, List<SizeSuggestion>> suggestions,
            string locale, string search)
        {
            GUILayout.Label(Hint, _styles.Description);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            IEnumerable<KeyValuePair<string, List<SizeSuggestion>>> shown = suggestions
                .Where(pair => string.IsNullOrEmpty(search)
                    || pair.Key.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(pair => pair.Value.All(row => row.IsFitting))
                .ThenBy(pair => pair.Key);

            foreach ((string key, List<SizeSuggestion> rows) in shown)
            {
                (List<SizeSuggestion> Rows, float Size)? picked = DrawStyle(key, rows, locale);

                if (picked.HasValue)
                    return picked;
            }

            return null;
        }

        // One line for a style that fits everywhere: its size or its range and the room it has left.
        private static string FoldedSummary(List<SizeSuggestion> rows)
        {
            float smallest = rows.Min(row => row.Size);
            float largest = rows.Max(row => row.Size);
            string sizes = Mathf.Approximately(smallest, largest)
                ? string.Format(FoldedSizeFormat, smallest)
                : string.Format(FoldedRangeFormat, smallest, largest);
            string more = rows.TrueForAll(row => row.IsCapped)
                ? SizeSuggestion.OrMore
                : string.Empty;

            return sizes + string.Format(FitsFormat, rows.Min(row => row.Fits.GetValueOrDefault())) + more;
        }

        // The smallest of the per-language maximums, so every language fits. None when one language fits nowhere.
        private static float? AllFit(List<SizeSuggestion> rows) => rows.All(row => row.Fits.HasValue)
            ? rows.Min(row => row.Fits.Value)
            : null;

        private (List<SizeSuggestion> Rows, float Size)? DrawStyle(string key, List<SizeSuggestion> rows,
            string locale)
        {
            (List<SizeSuggestion> Rows, float Size)? picked = null;
            float? allFit = AllFit(rows);
            bool isFitting = rows.TrueForAll(row => row.IsFitting);

            // A style that fits everywhere needs no attention, so it stays one line until asked for.
            bool isFolded = isFitting
                && locale == null
                && !_expanded.Contains(key);

            // A larger size is a design choice, not a fix, so the buttons only offer to shrink what does not fit.
            if (isFitting)
                allFit = null;

            EditorWindowChrome.BeginCard(_styles);

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorMetrics.RowHeight)))
            {
                TextFitWidgets.Icon(rows.All(row => row.IsFitting)
                    ? EditorIcons.Success
                    : EditorIcons.Error);
                GUILayout.Space(EditorMetrics.TightGap);
                TextFitWidgets.Label(key, _styles.NameBold);

                if (allFit.HasValue
                    && TextFitWidgets.PillButton(_styles, string.Format(ApplyAllFormat, allFit.Value), AllFitTooltip))
                    picked = (rows, allFit.Value);

                if (isFitting
                    && locale == null)
                    DrawFoldButton(key, isFolded);
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);

            if (isFolded)
            {
                TextFitWidgets.Label(FoldedSummary(rows), _styles.Detail);
                EditorWindowChrome.EndCard();
                EditorGUILayout.Space(EditorMetrics.ItemGap);

                return picked;
            }

            int shown = 0;

            foreach (SizeSuggestion row in rows)
            {
                if (locale != null
                    && row.Locale != locale)
                    continue;

                // A row that would only repeat the button above stays quiet.
                bool hasOwnButton = !allFit.HasValue
                    || !Mathf.Approximately(row.Fits.GetValueOrDefault(), allFit.Value);

                if (DrawRow(key, row, shown++, hasOwnButton)
                    && row.Fits.HasValue)
                    picked = (new List<SizeSuggestion> { row }, row.Fits.Value);
            }

            EditorWindowChrome.EndCard();
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            return picked;
        }

        private void DrawFoldButton(string key, bool isFolded)
        {
            string label = isFolded
                ? ShowLabel
                : HideLabel;

            if (!TextFitWidgets.PillButton(_styles, label, ShowTooltip))
                return;

            if (isFolded)
                _expanded.Add(key);
            else
                _expanded.Remove(key);
        }

        // Returns true when the button to use the fitting size was clicked. Every second row sits on a stripe.
        private bool DrawRow(string key, SizeSuggestion row, int index, bool hasOwnButton)
        {
            string language = TextFitLocales.NameOf(row.Locale);
            Color fill = row.IsFitting
                ? EditorTableStyles.OkBadgeColor
                : EditorTableStyles.DangerBadgeColor;

            using (EditorGUILayout.HorizontalScope scope = new(GUILayout.Height(EditorMetrics.RowHeight)))
            {
                TextFitWidgets.RowBackground(scope.rect, index);
                GUILayout.Space(EditorMetrics.TightGap);
                TextFitWidgets.Badge(_styles, row.Locale, EditorTableStyles.NeutralBadgeColor,
                    TextFitWidgets.LocaleWidth, language);
                GUILayout.Space(EditorMetrics.TightGap);
                TextFitWidgets.Badge(_styles, row.Size.ToString(), fill, SizeWidth,
                    string.Format(SizeTooltipFormat, key, language, row.Size));
                GUILayout.Space(EditorMetrics.TightGap);

                if (!row.Fits.HasValue)
                {
                    TextFitWidgets.Label(NoFitText, _styles.Detail, string.Format(NoFitTooltipFormat, key, language));
                    return false;
                }

                string fits = row.IsCapped
                    ? string.Format(FitsFormat, row.Fits.Value) + SizeSuggestion.OrMore
                    : string.Format(FitsFormat, row.Fits.Value);

                TextFitWidgets.Label(fits, _styles.Detail);

                return hasOwnButton
                    && !row.IsFitting
                    && TextFitWidgets.PillButton(_styles, string.Format(ApplyFormat, row.Fits.Value),
                        string.Format(ApplyTooltipFormat, key, language, row.Fits.Value));
            }
        }
    }
}