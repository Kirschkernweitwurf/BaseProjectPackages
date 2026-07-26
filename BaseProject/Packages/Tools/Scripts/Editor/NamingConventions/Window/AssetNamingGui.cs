using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Row metrics, label styles and the colored section headers of the asset naming window.
    /// Pure presentation.
    /// </summary>
    public static class AssetNamingGui
    {
        /// <summary>Horizontal padding between columns.</summary>
        public const float Padding = 6f;

        /// <summary>Height of a single result row.</summary>
        public const float RowHeight = 22f;

        private const float BadgeHeight = 16f;
        private const float BadgeWidth = 30f;
        private const float SectionHeight = 22f;
        private const float StripeWidth = 3f;

        /// <summary>Blue accent of the Rules section.</summary>
        public static readonly Color RulesAccent = new(0.33f, 0.52f, 0.74f);

        /// <summary>Calm grey accent of the Dismissed section.</summary>
        public static readonly Color DismissedAccent = new(0.55f, 0.55f, 0.58f);

        /// <summary>Teal accent of the Scan Results section.</summary>
        public static readonly Color ResultsAccent = new(0.26f, 0.62f, 0.58f);

        /// <summary>Light green accent of the History section.</summary>
        public static readonly Color HistoryAccent = new(0.62f, 0.78f, 0.5f);

        /// <summary>Zebra striping of result, dismissed and history rows.</summary>
        public static readonly Color EvenRowColor = new(0f, 0f, 0f, 0.14f);

        private static GUIStyle _badgeStyle;
        private static GUIStyle _countStyle;
        private static GUIStyle _detailStyle;
        private static GUIStyle _foldoutStyle;
        private static GUIStyle _nameStyle;

        /// <summary>Right aligned mini counter.</summary>
        public static GUIStyle CountStyle => _countStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        /// <summary>Dimmed style for secondary columns like rule, reason, path and time.</summary>
        public static GUIStyle DetailStyle => _detailStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft
        };

        /// <summary>Style of asset names.</summary>
        public static GUIStyle NameStyle => _nameStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft
        };

        private static GUIStyle BadgeStyle => _badgeStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            }
        };

        private static GUIStyle FoldoutStyle => _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold
        };

        /// <summary>
        /// Draws a tinted, color coded section header with a count badge and returns the new
        /// expanded state.
        /// </summary>
        public static bool DrawSectionHeader(bool expanded, string label, int count, Color accent)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, SectionHeight, GUILayout.ExpandWidth(true));
            rect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 2f);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(accent.r, accent.g, accent.b, 0.16f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, StripeWidth, rect.height), accent);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0f, 0f, 0f, 0.25f));
            }

            Rect foldoutRect = new(rect.x + 10f, rect.y + 2f, rect.width - BadgeWidth - 24f, rect.height - 4f);
            Rect badgeRect = new(rect.xMax - BadgeWidth - 6f, rect.y + (rect.height - BadgeHeight) * 0.5f,
                BadgeWidth, BadgeHeight);

            bool result = EditorGUI.Foldout(foldoutRect, expanded, label, true, FoldoutStyle);

            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(badgeRect, accent);

            GUI.Label(badgeRect, count.ToString(), BadgeStyle);

            return result;
        }

        /// <summary>Draws the zebra striping of a row.</summary>
        public static void DrawRowBackground(Rect row, int index)
        {
            if (index % 2 != 0)
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(row, EvenRowColor);
        }
    }
}
