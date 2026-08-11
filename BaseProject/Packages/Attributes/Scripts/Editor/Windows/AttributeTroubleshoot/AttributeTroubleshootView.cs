using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>
    /// Draws the grouped findings of the troubleshoot window. Pure presentation: it reports which type
    /// the user clicked and leaves opening the script to the window.
    /// </summary>
    internal static class AttributeTroubleshootView
    {
        private const float HeaderHeight = 22f;
        private const float HeaderIconSize = 16f;
        private const float IconSize = 14f;
        private const float Indent = 20f;
        private const float RowSpacing = 2f;
        private const float SuccessIconSize = 48f;
        private const float SuccessSpacing = 8f;
        private const string SuccessSubtitle = "Every attribute points at something it can use.";
        private const string SuccessTitle = "No problems found.";

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
        public static Type DrawGroups(List<AttributeIssueGroup> groups, string search, bool errorsOnly,
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

                foreach (AttributeIssue issue in visible)
                    DrawIssue(issue, styles);

                GUILayout.Space(RowSpacing);
            }

            return clicked;
        }

        /// <summary>Draws the centered empty state shown when nothing is wrong.</summary>
        /// <param name="styles">The built styles.</param>
        public static void DrawSuccess(AttributeTroubleshootStyles styles)
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
            => value != null && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        // Returns true when the header was clicked.
        private static bool DrawHeader(AttributeIssueGroup group, int count, AttributeTroubleshootStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, HeaderHeight);
            EditorGUI.DrawRect(rect, AttributeTroubleshootStyles.Header);

            Rect iconRect = new(rect.x + RowSpacing, rect.y + (rect.height - HeaderIconSize) * 0.5f,
                HeaderIconSize, HeaderIconSize);

            Texture icon = AttributeTroubleshootStyles.ScriptTexture;
            if (icon != null)
                GUI.DrawTexture(iconRect, icon);

            Rect labelRect = new(iconRect.xMax + RowSpacing * 2f, rect.y,
                rect.width - iconRect.xMax - RowSpacing * 2f, rect.height);

            GUI.Label(labelRect, $"{group.DisplayName}  ({count})", styles.Name);

            // Sample groups have no script behind them, so they do not advertise a click.
            if (group.Type != null)
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return Event.current.type == EventType.MouseDown
                && Event.current.button == 0
                && rect.Contains(Event.current.mousePosition);
        }

        private static void DrawIssue(in AttributeIssue issue, AttributeTroubleshootStyles styles)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Indent);

            Texture icon = issue.Severity == EAttributeIssueSeverity.Error
                ? AttributeTroubleshootStyles.ErrorTexture
                : AttributeTroubleshootStyles.WarningTexture;

            if (icon != null)
                GUILayout.Label(icon, GUILayout.Width(IconSize), GUILayout.Height(IconSize));

            EditorGUILayout.BeginVertical();
            GUILayout.Label($"[{issue.AttributeName}] on {issue.MemberName}", styles.Member);
            GUILayout.Label(issue.Message, styles.Message);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(RowSpacing);
        }
    }
}