using System;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Shared drawing for <see cref="TitleAttribute"/>. Used by <see cref="TitleHandler"/> for plain
    /// titles and by <see cref="AttributePackageEditor"/> for collapsible titles, so both look the same.
    /// </summary>
    public static class TitleRenderer
    {
        private const string KeySeparator = ".";
        private const string FoldoutKeyPrefix = "TITLE";

        private static readonly Color DefaultLine = new(0.5f, 0.5f, 0.5f, 0.5f);

        private static GUIStyle _labelStyle;
        private static GUIStyle _foldoutStyle;

        private static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.boldLabel);

        private static GUIStyle FoldoutStyle => _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        /// <summary>Draws a plain bold title with an underline.</summary>
        public static void DrawPlain(TitleAttribute attribute)
        {
            bool hasColor =
                ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color color);

            GUILayout.Space(6f);

            LabelStyle.normal.textColor = hasColor
                ? color
                : EditorStyles.boldLabel.normal.textColor;

            EditorGUILayout.LabelField(attribute.Title, LabelStyle);
            DrawUnderline(hasColor, color);
            GUILayout.Space(2f);
        }

        /// <summary>
        /// Draws a collapsible bold title with an underline and returns its expanded state. The state is
        /// stored per owner type and title in <see cref="EditorPrefs"/>.
        /// </summary>
        public static bool DrawCollapsible(Type ownerType, TitleAttribute attribute)
        {
            bool hasColor =
                ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color color);

            GUILayout.Space(6f);

            string key = ownerType.FullName + KeySeparator + FoldoutKeyPrefix + KeySeparator + attribute.Title;
            bool stored = EditorPrefs.GetBool(key, attribute.DefaultExpanded);
            bool expanded = stored;

            Color previous = FoldoutStyle.normal.textColor;
            if (hasColor)
            {
                FoldoutStyle.normal.textColor = color;
                FoldoutStyle.onNormal.textColor = color;
            }

            expanded = EditorGUILayout.Foldout(expanded, attribute.Title, true, FoldoutStyle);

            if (hasColor)
            {
                FoldoutStyle.normal.textColor = previous;
                FoldoutStyle.onNormal.textColor = previous;
            }

            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            DrawUnderline(hasColor, color);
            GUILayout.Space(2f);
            return expanded;
        }

        private static void DrawUnderline(bool hasColor, Color color)
        {
            Rect lineRect = EditorGUILayout.GetControlRect(false, 3f);
            lineRect.height = 1f;
            EditorGUI.DrawRect(lineRect, hasColor
                ? color
                : DefaultLine);
        }

    }
}