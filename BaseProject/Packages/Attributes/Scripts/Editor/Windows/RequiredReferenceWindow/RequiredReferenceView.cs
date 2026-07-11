using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>Renders the grouped list of missing references. Returns the object a click targeted.</summary>
    public static class RequiredReferenceView
    {
        private const float AccentWidth = 3f;
        private const float BadgeInset = 4f;
        private const float BadgePadding = 6f;
        private const float BadgeReserve = 60f;
        private const float EmptyIconSize = 32f;
        private const float GroupSpacing = 8f;
        private const float HeaderHeight = 24f;
        private const float IconSize = 15f;
        private const float LabelGap = 5f;
        private const float LeftPadding = 8f;
        private const float RowHeight = 20f;
        private const float RowIndent = 22f;

        /// <summary>Draws every group filtered by search. Returns the clicked owner, or null.</summary>
        public static GameObject DrawGroups(List<RequiredReferenceGroup> groups,
            string search,
            RequiredReferenceStyles styles,
            out bool anyShown)
        {
            GameObject clicked = null;
            anyShown = false;

            foreach (RequiredReferenceGroup group in groups)
            {
                if (!Matches(group, search, out List<RequiredReferenceEntry> visible))
                    continue;

                anyShown = true;

                clicked = DrawHeader(group, visible.Count, styles) ?? clicked;

                foreach (RequiredReferenceEntry entry in visible)
                    clicked = DrawRow(group.Owner, entry.DisplayName, styles) ?? clicked;

                GUILayout.Space(GroupSpacing);
            }

            return clicked;
        }

        /// <summary>Draws the success state shown when nothing is missing.</summary>
        public static void DrawEmptyState(RequiredReferenceStyles styles)
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginVertical();

            GUILayout.Label(new GUIContent(styles.SuccessTexture),
                GUILayout.Width(EmptyIconSize),
                GUILayout.Height(EmptyIconSize));

            GUILayout.Label("All required references are assigned.",
                styles.Empty);

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
        }

        private static GameObject DrawHeader(RequiredReferenceGroup group,
            int count,
            RequiredReferenceStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, HeaderHeight);

            EditorGUI.DrawRect(rect, RequiredReferenceStyles.Header);

            Rect iconRect = new(rect.x + LeftPadding,
                rect.y + (rect.height - IconSize) * .5f,
                IconSize,
                IconSize);

            GUI.DrawTexture(iconRect, styles.ObjectTexture);

            string name = group.Owner != null
                ? group.Owner.name
                : "<missing object>";

            Rect labelRect = new(iconRect.xMax + LabelGap,
                rect.y,
                rect.width - RowIndent - BadgeReserve,
                rect.height);

            GUI.Label(labelRect, name, styles.Name);

            DrawBadge(rect, count, styles);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return Clicked(rect, group.Owner);
        }

        private static void DrawBadge(Rect header,
            int count,
            RequiredReferenceStyles styles)
        {
            string text = count.ToString();

            float width =
                styles.Badge.CalcSize(new GUIContent(text)).x + BadgePadding * 2f;

            Rect badge = new(header.xMax - width - LeftPadding,
                header.y + BadgeInset,
                width,
                header.height - BadgeInset * 2f);

            EditorGUI.DrawRect(badge, RequiredReferenceStyles.Accent);

            GUI.Label(badge, text, styles.Badge);
        }

        private static GameObject DrawRow(GameObject owner,
            string detail,
            RequiredReferenceStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, AccentWidth, rect.height),
                RequiredReferenceStyles.Accent);

            Rect icon = new(rect.x + RowIndent,
                rect.y + (rect.height - IconSize) * .5f,
                IconSize,
                IconSize);

            GUI.DrawTexture(icon, styles.ErrorTexture);

            Rect label = new(icon.xMax + LabelGap,
                rect.y,
                rect.width - icon.xMax - LabelGap,
                rect.height);

            GUI.Label(label, detail, styles.Path);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return Clicked(rect, owner);
        }

        private static GameObject Clicked(Rect rect, GameObject owner)
        {
            if (Event.current.type != EventType.MouseDown)
                return null;

            if (!rect.Contains(Event.current.mousePosition))
                return null;

            Event.current.Use();

            return owner;
        }

        private static bool Matches(RequiredReferenceGroup group, string search,
            out List<RequiredReferenceEntry> visible)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                visible = group.Entries;
                return true;
            }

            search = search.ToLowerInvariant();

            if (group.Owner != null && group.Owner.name.ToLowerInvariant().Contains(search))
            {
                visible = group.Entries;
                return true;
            }

            visible = group.Entries.FindAll(entry =>
                entry.DisplayName.ToLowerInvariant().Contains(search));

            return visible.Count > 0;
        }
    }
}