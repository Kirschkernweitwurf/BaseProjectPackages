using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEditor.Localization.UI;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Draws one card per group of problems: what it is, where its box lives, buttons to fix or open the box and one
    /// striped row per problem with the text and why it does not fit. A text group has a row per language, a box
    /// group a row per text, each with its own button to open the text.
    /// </summary>
    internal sealed class TextFitIssueView
    {
        private const string AutoSizeLabel = "Auto Size";
        private const string AutoSizeTooltipFormat = "Lets the box shrink from size {0} down to {1}, so a long "
            + "translation fits instead of spilling out. Opens the box and undo works.";
        private const string EditLabel = "Edit Text";
        private const string EditTooltipFormat = "Opens '{0}' in the string tables, so the text can be shortened.";
        private const string PingLabel = "Ping Source";
        private const string PingTooltip = "Built in code from this asset. Pings it in the Project window.";
        private const string ReasonFormat = "{0}  ·  {1}";
        private const string StyleTooltip = "Text style: font and size per language from the Text Styles table.";

        private readonly EditorWindowStyles _styles;

        /// <summary>Creates the view, drawing with <paramref name="styles"/>.</summary>
        internal TextFitIssueView(EditorWindowStyles styles) => _styles = styles;

        /// <summary>
        /// Draws every group with a problem in <paramref name="locale"/> that matches <paramref name="search"/> and
        /// returns how many it drew.
        /// </summary>
        /// <param name="groups">The groups to draw.</param>
        /// <param name="locale">The language code to show or null for every language.</param>
        /// <param name="search">Text to look for or empty to show everything.</param>
        /// <param name="localeFill">
        /// The fill of the language badges or null to leave them out, for a page that only has one language.
        /// </param>
        internal int Draw(List<TextFitGroup> groups, string locale, string search, Color? localeFill)
        {
            int drawn = 0;

            foreach (TextFitGroup group in groups)
            {
                List<TextFitIssue> visible = group.Visible(locale, search);

                if (visible.Count == 0)
                    continue;

                DrawGroup(group, visible, localeFill);
                drawn++;
            }

            return drawn;
        }

        // Opening a prefab or a window rebuilds the GUI, so the rest of this pass is skipped.
        private static void Act(Action action)
        {
            action();
            GUIUtility.ExitGUI();
        }

        // A row of a box group names its text, since the card title names the box.
        private static string ReasonOf(TextFitIssue issue, bool isByBox)
        {
            TextSample sample = issue.Sample;
            string reason = string.IsNullOrEmpty(sample.Arguments)
                ? issue.Reason
                : string.Format(ReasonFormat, issue.Reason, sample.Arguments);

            return isByBox
                ? string.Format(ReasonFormat, sample.Source, reason)
                : reason;
        }

        private void DrawGroup(TextFitGroup group, List<TextFitIssue> issues, Color? localeFill)
        {
            TextSample sample = group.Sample;
            string title = group.IsByBox
                ? sample.Box.Location.Name
                : sample.Source;

            float titleEnd;

            EditorWindowChrome.BeginCard(_styles);

            using (new EditorGUILayout.HorizontalScope(GUILayout.Height(EditorMetrics.RowHeight)))
            {
                TextFitWidgets.Label(title, _styles.NameBold);
                titleEnd = GUILayoutUtility.GetLastRect().xMax;

                if (sample.Box.HasStyle)
                {
                    TextFitWidgets.Badge(_styles, sample.Box.StyleKey, EditorTableStyles.NeutralBadgeColor,
                        EditorTableStyles.MinBadgeWidth, StyleTooltip);
                    GUILayout.Space(EditorMetrics.TightGap);
                }

                DrawBoxActions(sample.Box);

                // A box group holds many texts, so each of its rows opens its own.
                if (!group.IsByBox)
                {
                    GUILayout.Space(EditorMetrics.TightGap);
                    DrawTextAction(sample);
                }
            }

            // The path stays under the title, clear of the buttons beside it.
            TextFitWidgets.PathLabel(sample.Box.Location, sample.Area, _styles.Detail, titleEnd);
            EditorGUILayout.Space(EditorMetrics.TightGap);

            for (int i = 0; i < issues.Count; i++)
                DrawIssue(issues[i], i, localeFill, group.IsByBox);

            EditorWindowChrome.EndCard();
            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        // The fix comes first, since it is the one click that ends the problem.
        private void DrawBoxActions(TextBox box)
        {
            if (TextFitAutoSize.CanFix(box))
            {
                string tooltip = string.Format(AutoSizeTooltipFormat, box.Text.fontSize,
                    TextFitAutoSize.MinSizeOf(box));

                if (TextFitWidgets.PillButton(_styles, AutoSizeLabel, tooltip))
                    Act(() => TextFitAutoSize.Apply(box));

                GUILayout.Space(EditorMetrics.TightGap);
            }

            TextFitWidgets.ShowBoxButton(_styles, box.Location);
        }

        private void DrawTextAction(TextSample sample)
        {
            if (sample.Entry != null
                && !sample.Entry.IsEmpty)
            {
                if (TextFitWidgets.PillButton(_styles, EditLabel, string.Format(EditTooltipFormat, sample.Source)))
                    Act(() => LocalizationTablesWindow.ShowWindow(sample.Entry.TableReference,
                        sample.Entry.TableEntryReference));

                return;
            }

            if (sample.Context != null
                && TextFitWidgets.PillButton(_styles, PingLabel, PingTooltip))
                EditorGUIUtility.PingObject(sample.Context);
        }

        // Every second row sits on a stripe, so the eye keeps a text and its reason together.
        private void DrawIssue(TextFitIssue issue, int index, Color? localeFill, bool isByBox)
        {
            TextSample sample = issue.Sample;
            float indent = localeFill.HasValue
                ? TextFitWidgets.LocaleWidth + EditorMetrics.TightGap
                : 0f;

            Rect row = EditorGUILayout.BeginVertical(GUILayout.MinHeight(EditorMetrics.RowHeight));
            TextFitWidgets.RowBackground(row, index);
            EditorGUILayout.Space(EditorMetrics.TightGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorMetrics.TightGap);

                if (localeFill.HasValue)
                {
                    TextFitWidgets.Badge(_styles, issue.Locale, localeFill.Value, TextFitWidgets.LocaleWidth,
                        TextFitLocales.NameOf(issue.Locale));
                    GUILayout.Space(EditorMetrics.TightGap);
                }

                TextFitWidgets.Label(TextFitWidgets.Plain(sample.Text), _styles.Name, sample.Text);

                if (isByBox)
                {
                    DrawTextAction(sample);
                    GUILayout.Space(EditorMetrics.TightGap);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorMetrics.TightGap + indent);
                TextFitWidgets.Label(ReasonOf(issue, isByBox), _styles.Detail);
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);
            EditorGUILayout.EndVertical();
        }
    }
}