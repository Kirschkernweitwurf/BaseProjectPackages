using System;
using Base.AttributePackage.Editor.Handlers;
using Base.AttributePackage.Editor.Inspectors;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Shared drawing for <see cref="TitleAttribute"/>. Used by <see cref="TitleHandler"/> for plain
    /// titles and by <see cref="AttributePackageEditor"/> for collapsible titles, so both look the same.
    /// </summary>
    internal static class TitleRenderer
    {
        private const string FoldoutKeyPrefix = "TITLE";
        private const float LineHeight = 1f;
        private const float SpaceAbove = 6f;
        private const float SpaceBelow = 2f;
        private const float UnderlineRowHeight = 3f;

        private static GUIStyle FoldoutStyle => _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        private static GUIStyle LabelStyle => _labelStyle ??= new GUIStyle(EditorStyles.boldLabel);

        private static readonly Color DefaultLine = new(0.5f, 0.5f, 0.5f, 0.5f);

        private static GUIStyle _foldoutStyle;
        private static GUIStyle _labelStyle;

        /// <summary>Draws a plain bold title with an underline.</summary>
        /// <param name="attribute">The title to draw.</param>
        /// <param name="title">The resolved title text.</param>
        public static void DrawPlain(TitleAttribute attribute, string title)
        {
            bool hasColor = TryResolveColor(attribute, out Color color);
            GUILayout.Space(SpaceAbove);

            LabelStyle.normal.textColor = hasColor
                ? color
                : EditorStyles.boldLabel.normal.textColor;

            EditorGUILayout.LabelField(title, LabelStyle);
            DrawUnderline(hasColor, color);
        }

        /// <summary>
        /// Draws a collapsible bold title with an underline and returns its expanded state. The state is
        /// stored per owner type and title in <see cref="EditorPrefs"/>.
        /// </summary>
        /// <param name="title">The resolved title text.</param>
        public static bool DrawCollapsible(Type ownerType, TitleAttribute attribute, string title)
        {
            bool hasColor = TryResolveColor(attribute, out Color color);
            GUILayout.Space(SpaceAbove);

            // The state key uses the authored title rather than the resolved one, so a title that
            // computes its text does not lose its expanded state every time that text changes.
            string key = StateKey.For(ownerType, FoldoutKeyPrefix, attribute.Title);
            bool stored = EditorPrefs.GetBool(key, attribute.DefaultExpanded);

            Color previous = FoldoutStyle.normal.textColor;
            if (hasColor)
            {
                FoldoutStyle.normal.textColor = color;
                FoldoutStyle.onNormal.textColor = color;
            }

            bool expanded = EditorGUILayout.Foldout(stored, title, true, FoldoutStyle);

            if (hasColor)
            {
                FoldoutStyle.normal.textColor = previous;
                FoldoutStyle.onNormal.textColor = previous;
            }

            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            DrawUnderline(hasColor, color);
            return expanded;
        }

        private static bool TryResolveColor(TitleAttribute attribute, out Color color)
            => ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out color);

        private static void DrawUnderline(bool hasColor, Color color)
        {
            Rect lineRect = EditorGUILayout.GetControlRect(false, UnderlineRowHeight);
            lineRect.height = LineHeight;

            EditorGUI.DrawRect(lineRect, hasColor
                ? color
                : DefaultLine);

            GUILayout.Space(SpaceBelow);
        }
    }
}