using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Draws the grouped findings. Pure presentation: it reports which type the user clicked and leaves
    /// opening the script to the window.
    /// </summary>
    /// <remarks>
    /// One band per type with its findings under it, each marked by a colored bar rather than only by an
    /// icon. A list where errors and warnings differ only by a sixteen pixel glyph is a list you have to
    /// read to skim, which is the opposite of what a report is for.
    /// </remarks>
    internal static class AttributeTroubleshootView
    {
        private const float BarWidth = 3f;
        private const float CountWidth = 70f;
        private const string CountFormat = "{0} {1}";
        private const float GroupSpacing = 8f;
        private const float HeaderHeight = 24f;
        private const float HeaderIconSize = 16f;
        private const float IconSize = 14f;
        private const float Indent = 10f;
        private const float MessageIndent = 24f;
        private const float RowPadding = 4f;
        private const float RowSpacing = 2f;
        private const float SuccessIconSize = 48f;
        private const float SuccessSpacing = 8f;
        private const string SuccessSubtitle = "Every attribute points at something it can use.";
        private const string SuccessTitle = "No problems found.";

        private static readonly GUIContent Scratch = new();

        /// <summary>
        /// Draws every group whose type name or findings match the filter, and returns the type of the
        /// header the user clicked, or null.
        /// </summary>
        /// <param name="groups">The groups to draw.</param>
        /// <param name="search">Free text filter, empty to show everything.</param>
        /// <param name="errorsOnly">Whether warnings are hidden.</param>
        /// <param name="styles">The built styles.</param>
        /// <param name="anyShown">Whether at least one group passed the filter.</param>
        /// <returns>The clicked type, or null.</returns>
        internal static Type DrawGroups(List<AttributeIssueGroup> groups, string search, bool errorsOnly,
            AttributeTroubleshootStyles styles, out bool anyShown)
        {
            Type clicked = null;
            anyShown = false;

            foreach (AttributeIssueGroup group in groups)
            {
                List<AttributeIssue> visible = Filter(group, search, errorsOnly);

                if (visible.Count == 0)
                    continue;

                anyShown = true;

                if (DrawHeader(group, visible.Count, styles) && group.Type != null)
                    clicked = group.Type;

                for (int i = 0; i < visible.Count; i++)
                    DrawIssue(visible[i], styles, i);

                GUILayout.Space(GroupSpacing);
            }

            return clicked;
        }

        /// <summary>Draws the centered empty state shown when nothing is wrong.</summary>
        /// <param name="styles">The built styles.</param>
        internal static void DrawSuccess(AttributeTroubleshootStyles styles)
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Texture icon = AttributeTroubleshootStyles.SuccessTexture;

            if (icon != null)
                GUILayout.Label(icon, GUILayout.Width(SuccessIconSize), GUILayout.Height(SuccessIconSize));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(SuccessSpacing);
            GUILayout.Label(SuccessTitle, styles.SuccessTitle);
            GUILayout.Label(SuccessSubtitle, styles.SuccessSubtitle);

            GUILayout.FlexibleSpace();
        }

        private static List<AttributeIssue> Filter(AttributeIssueGroup group, string search, bool errorsOnly)
        {
            List<AttributeIssue> visible = new();

            foreach (AttributeIssue issue in group.Issues)
            {
                if (errorsOnly && issue.Severity != EAttributeIssueSeverity.Error)
                    continue;

                if (Matches(group, issue, search))
                    visible.Add(issue);
            }

            return visible;
        }

        private static bool Matches(AttributeIssueGroup group, in AttributeIssue issue, string search)
        {
            if (string.IsNullOrEmpty(search))
                return true;

            return Contains(group.DisplayName, search)
                || Contains(issue.MemberName, search)
                || Contains(issue.AttributeName, search)
                || Contains(issue.Message, search);
        }

        private static bool Contains(string value, string search)
            => value != null && value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private static string Plural(int count, string word) => count == 1
            ? word
            : word + "s";

        // Returns true when the header was clicked.
        private static bool DrawHeader(AttributeIssueGroup group, int count,
            AttributeTroubleshootStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, HeaderHeight);
            bool hovered = rect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, AttributeTroubleshootStyles.Header);

                if (hovered && group.Type != null)
                    EditorGUI.DrawRect(rect, styles.Hover);
            }

            Rect icon = new(rect.x + RowPadding, rect.y + (rect.height - HeaderIconSize) * 0.5f,
                HeaderIconSize, HeaderIconSize);

            Texture script = AttributeTroubleshootStyles.ScriptTexture;

            if (script != null && Event.current.type == EventType.Repaint)
                GUI.DrawTexture(icon, script);

            Rect label = new(icon.xMax + RowPadding, rect.y,
                rect.width - icon.xMax - CountWidth - RowPadding, rect.height);

            GUI.Label(label, group.DisplayName, styles.Name);

            Scratch.text = string.Format(CountFormat, count, Plural(count, "finding"));
            GUI.Label(new Rect(rect.xMax - CountWidth - RowPadding, rect.y, CountWidth, rect.height),
                Scratch, styles.Count);

            // Test case groups have no script behind them, so they do not advertise a click.
            if (group.Type != null)
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return Event.current.type == EventType.MouseDown
                && Event.current.button == 0
                && rect.Contains(Event.current.mousePosition);
        }

        private static void DrawIssue(in AttributeIssue issue, AttributeTroubleshootStyles styles, int row)
        {
            bool isError = issue.Severity == EAttributeIssueSeverity.Error;

            Scratch.text = issue.Message;

            float messageWidth = Mathf.Max(EditorGUIUtility.currentViewWidth - MessageIndent * 2f
                - Indent, MessageIndent);

            float height = EditorGUIUtility.singleLineHeight
                + styles.Message.CalcHeight(Scratch, messageWidth) + RowPadding;

            Rect rect = EditorGUILayout.GetControlRect(false, height);

            if (Event.current.type == EventType.Repaint)
            {
                if (row % 2 != 0)
                    EditorGUI.DrawRect(rect, styles.Stripe);

                EditorGUI.DrawRect(new Rect(rect.x + Indent, rect.y, BarWidth, rect.height), isError
                    ? AttributeTroubleshootStyles.Error
                    : AttributeTroubleshootStyles.Warning);
            }

            Rect icon = new(rect.x + Indent + BarWidth + RowPadding, rect.y + RowPadding * 0.5f,
                IconSize, IconSize);

            Texture severity = isError
                ? AttributeTroubleshootStyles.ErrorTexture
                : AttributeTroubleshootStyles.WarningTexture;

            if (severity != null && Event.current.type == EventType.Repaint)
                GUI.DrawTexture(icon, severity);

            Rect member = new(icon.xMax + RowPadding, rect.y, rect.width - icon.xMax - RowPadding,
                EditorGUIUtility.singleLineHeight);

            GUI.Label(member, $"[{issue.AttributeName}] on {issue.MemberName}", styles.Member);

            Rect message = new(rect.x + MessageIndent + Indent, member.yMax,
                Mathf.Max(rect.width - MessageIndent * 2f - Indent, MessageIndent),
                rect.height - member.height);

            GUI.Label(message, issue.Message, styles.Message);

            GUILayout.Space(RowSpacing);
        }
    }
}