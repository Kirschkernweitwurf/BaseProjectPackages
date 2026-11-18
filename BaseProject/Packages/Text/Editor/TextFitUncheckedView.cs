using System;
using System.Collections.Generic;
using System.Linq;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Draws the boxes with words in them that the check never renders, each with a button to open it and one to
    /// ignore it for good, so every text that reaches a player is either checked or knowingly left out.
    /// </summary>
    internal sealed class TextFitUncheckedView
    {
        private const string CheckAgainLabel = "Check Again";
        private const string CheckAgainTooltip = "Lists this box again.";
        private const string IgnoreLabel = "Ignore";
        private const string IgnoreTooltip = "Never lists this box again, like a debug label or a placeholder no "
            + "player sees. Saved in the project settings, so it counts for the whole team.";
        private const string QuoteFormat = "“{0}”";
        private const string ShowIgnoredFormat = "Show the {0} ignored boxes";

        private static readonly string Hint = "Nobody knows whether a translation fits these boxes, because the "
            + "check never renders them. A fixed text gets a "
            + ObjectNames.NicifyVariableName(nameof(LocalizeStringEvent)) + ". For a text set in code, mark its "
            + nameof(LocalizedString) + " field with [" + nameof(ShownInAttribute).Replace(nameof(Attribute),
                string.Empty) + "] or add an " + nameof(ITextSampleSource) + " that knows how the code builds it. "
            + "Ignore what no player reads.";

        private readonly EditorWindowStyles _styles;

        /// <summary>Creates the view, drawing with <paramref name="styles"/>.</summary>
        internal TextFitUncheckedView(EditorWindowStyles styles) => _styles = styles;

        /// <summary>
        /// Draws the boxes that match <paramref name="search"/>, the ignored ones too while
        /// <paramref name="isShowingIgnored"/> is on and returns how many it drew.
        /// </summary>
        internal int Draw(List<UncheckedText> texts, string search, ref bool isShowingIgnored)
        {
            TextFitIgnoreList ignored = TextFitIgnoreList.instance;
            List<UncheckedText> matching = texts.Where(text => Matches(text, search)).ToList();
            List<UncheckedText> open = matching.Where(text => !ignored.IsIgnored(text.Location)).ToList();
            int ignoredCount = matching.Count - open.Count;

            GUILayout.Label(Hint, _styles.Description);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (open.Count > 0)
                DrawCard(open, false);

            if (ignoredCount == 0)
                return open.Count;

            isShowingIgnored = EditorGUILayout.ToggleLeft(string.Format(ShowIgnoredFormat, ignoredCount),
                isShowingIgnored, _styles.Detail);

            if (isShowingIgnored)
                DrawCard(matching.Where(text => ignored.IsIgnored(text.Location)).ToList(), true);

            return open.Count;
        }

        private static bool Matches(UncheckedText text, string search) => string.IsNullOrEmpty(search)
            || text.Text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
            || text.Location.Path.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private void DrawCard(List<UncheckedText> texts, bool isIgnored)
        {
            EditorWindowChrome.BeginCard(_styles);

            for (int i = 0; i < texts.Count; i++)
            {
                if (i > 0)
                {
                    EditorGUILayout.Space(EditorMetrics.ItemGap);
                    EditorWindowChrome.DrawSeparator();
                    EditorGUILayout.Space(EditorMetrics.ItemGap);
                }

                DrawText(texts[i], isIgnored);
            }

            EditorWindowChrome.EndCard();
            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        private void DrawText(UncheckedText text, bool isIgnored)
        {
            TextBoxLocation location = text.Location;
            float titleEnd;

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorMetrics.RowHeight)))
            {
                TextFitWidgets.Label(location.Name, _styles.NameBold, location.Path);
                titleEnd = GUILayoutUtility.GetLastRect().xMax;
                TextFitWidgets.ShowBoxButton(_styles, location);
                GUILayout.Space(EditorMetrics.TightGap);

                string label = isIgnored
                    ? CheckAgainLabel
                    : IgnoreLabel;
                string tooltip = isIgnored
                    ? CheckAgainTooltip
                    : IgnoreTooltip;

                if (TextFitWidgets.PillButton(_styles, label, tooltip))
                    TextFitIgnoreList.instance.SetIgnored(location, !isIgnored);
            }

            TextFitWidgets.PathLabel(location, null, _styles.Detail, titleEnd);
            TextFitWidgets.Label(string.Format(QuoteFormat, TextFitWidgets.Plain(text.Text)), _styles.Name, text.Text);
        }
    }
}