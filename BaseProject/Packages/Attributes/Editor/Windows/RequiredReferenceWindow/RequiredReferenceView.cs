using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>Renders the grouped list of issues. Returns the object a click targeted.</summary>
    internal static class RequiredReferenceView
    {
        private const float AccentWidth = 3f;
        private const float BadgeReserve = 60f;
        private const float GroupSpacing = 8f;
        private const float HeaderHeight = 24f;
        private const float IconSize = 15f;
        private const float LabelGap = 5f;
        private const float LeftPadding = 8f;
        private const string MissingOwnerName = "<missing object>";
        private const float RowHeight = 20f;
        private const float RowIndent = 22f;
        private const float SuccessGap = 8f;
        private const float SuccessIconSize = 48f;

        // Reused rather than cached with its texture: a GUIContent built once holds the icon it was
        // given forever, which defeats the fake-null check EditorIcons does on every access. The
        // image is re-read each draw instead, so a destroyed icon is replaced rather than drawn blank.
        private static readonly GUIContent SuccessContent = new();

        /// <summary>Draws every group filtered by search. Returns the clicked owner, or null.</summary>
        internal static Object DrawGroups(List<RequiredReferenceGroup> groups,
            string search,
            RequiredReferenceStyles styles,
            out bool anyShown)
        {
            Object clicked = null;
            anyShown = false;

            foreach (RequiredReferenceGroup group in groups)
            {
                if (!Matches(group, search, out List<RequiredReferenceEntry> visible))
                    continue;

                anyShown = true;

                Object header = DrawHeader(group, visible.Count, styles);
                if (header != null)
                    clicked = header;

                foreach (RequiredReferenceEntry entry in visible)
                {
                    Object row = DrawRow(group.Owner, entry.DisplayName, styles);
                    if (row != null)
                        clicked = row;
                }

                GUILayout.Space(GroupSpacing);
            }

            return clicked;
        }

        /// <summary>Draws the rewarding success state shown when nothing is missing.</summary>
        internal static void DrawSuccess(RequiredReferenceStyles styles)
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            SuccessContent.image = RequiredReferenceStyles.SuccessTexture;

            GUILayout.Label(SuccessContent,
                GUILayout.Width(SuccessIconSize),
                GUILayout.Height(SuccessIconSize));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(SuccessGap);

            GUILayout.Label("All references assigned", styles.SuccessTitle);
            GUILayout.Label("Everything is wired up. Nothing to fix.", styles.SuccessSubtitle);

            GUILayout.FlexibleSpace();
        }

        private static Object DrawHeader(RequiredReferenceGroup group,
            int count,
            RequiredReferenceStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, HeaderHeight);

            EditorGUI.DrawRect(rect, RequiredReferenceStyles.Header);

            Rect iconRect = new(rect.x + LeftPadding,
                rect.y + (rect.height - IconSize) * 0.5f,
                IconSize,
                IconSize);

            Texture icon = group.Owner != null
                ? AssetPreview.GetMiniThumbnail(group.Owner)
                : RequiredReferenceStyles.ObjectTexture;

            GUI.DrawTexture(iconRect, icon);

            string name = group.Owner != null
                ? group.Owner.name
                : MissingOwnerName;

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
            float width = EditorRows.MeasureBadge(text, styles.Badge);

            Rect cell = new(header.xMax - width - LeftPadding, header.y, width, header.height);

            EditorRows.DrawBadge(cell, text, RequiredReferenceStyles.Accent, styles.Badge);
        }

        private static Object DrawRow(Object owner,
            string detail,
            RequiredReferenceStyles styles)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, RowHeight);

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, AccentWidth, rect.height),
                RequiredReferenceStyles.Accent);

            Rect icon = new(rect.x + RowIndent,
                rect.y + (rect.height - IconSize) * 0.5f,
                IconSize,
                IconSize);

            GUI.DrawTexture(icon, RequiredReferenceStyles.ErrorTexture);

            Rect label = new(icon.xMax + LabelGap,
                rect.y,
                rect.width - icon.xMax - LabelGap,
                rect.height);

            GUI.Label(label, detail, styles.Path);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return Clicked(rect, owner);
        }

        private static Object Clicked(Rect rect, Object owner)
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

            if (group.Owner != null
                && group.Owner.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                visible = group.Entries;
                return true;
            }

            visible = group.Entries.FindAll(entry =>
                entry.DisplayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            return visible.Count > 0;
        }
    }
}