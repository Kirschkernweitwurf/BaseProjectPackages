using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Row metrics, label styles, colored section headers and the success block of the asset
    /// naming window. Pure presentation.
    /// </summary>
    internal static class AssetNamingGui
    {
        /// <summary>Horizontal padding between columns.</summary>
        public const float Padding = 6f;

        /// <summary>Height of a single table row.</summary>
        public const float RowHeight = 22f;

        private const float BadgeHeight = 16f;
        private const float BadgeWidth = 30f;
        /// <summary>Height of a group header inside a section.</summary>
        private const float GroupHeight = 18f;
        private const float SectionHeight = 22f;
        private const float StripeWidth = 3f;
        private const float SuccessGap = 8f;
        private const string SuccessIcon = "TestPassed";
        private const float SuccessIconSize = 20f;
        private const int SuccessTitleFontSize = 15;
        private const float SuccessTitleGap = 2f;

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

        private static GUIStyle GroupStyle => _groupStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontSize = EditorStyles.miniBoldLabel.fontSize,
            fontStyle = FontStyle.Bold
        };

        /// <summary>Green check icon, the same one the project health overviews use.</summary>
        private static Texture SuccessTexture => _successTexture ??= EditorGUIUtility.IconContent(SuccessIcon).image;

        private static GUIStyle SuccessTitleStyle => _successTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = SuccessTitleFontSize,
            normal =
            {
                textColor = SuccessTitleColor
            },
            hover =
            {
                textColor = SuccessTitleColor
            }
        };

        private static GUIStyle SuccessSubtitleStyle => _successSubtitleStyle ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal =
            {
                textColor = SuccessSubtitleColor
            },
            hover =
            {
                textColor = SuccessSubtitleColor
            }
        };

        /// <summary>Blue accent of the Rules section.</summary>
        public static readonly Color RulesAccent = new(0.33f, 0.52f, 0.74f);

        /// <summary>Calm gray accent of the Dismissed section.</summary>
        public static readonly Color DismissedAccent = new(0.55f, 0.55f, 0.58f);

        /// <summary>Teal accent of the Scan Results section.</summary>
        public static readonly Color ResultsAccent = new(0.26f, 0.62f, 0.58f);

        /// <summary>Light green accent of the History section.</summary>
        public static readonly Color HistoryAccent = new(0.62f, 0.78f, 0.5f);

        /// <summary>Line color of column dividers and table borders.</summary>
        public static readonly Color DividerColor = new(0f, 0f, 0f, 0.3f);

        /// <summary>Casing options, each written in the casing it stands for.</summary>
        public static readonly string[] StyleLabels =
        {
            "Any",
            "PascalCase",
            "camelCase",
            "UPPER_SNAKE_CASE",
            "lower_snake_case",
            "Pascal_Snake_Case"
        };

        /// <summary>Zebra striping of table rows.</summary>
        private static readonly Color EvenRowColor = new(0f, 0f, 0f, 0.14f);

        private static readonly Color HeaderColor = new(0f, 0f, 0f, 0.2f);
        private static readonly Color SuccessTitleColor = new(0.36f, 0.76f, 0.46f);
        private static readonly Color SuccessSubtitleColor = new(0.5f, 0.5f, 0.5f);

        private static GUIStyle _badgeStyle;
        private static GUIStyle _detailStyle;
        private static GUIStyle _foldoutStyle;
        private static GUIStyle _groupStyle;
        private static GUIStyle _nameStyle;
        private static GUIStyle _successSubtitleStyle;
        private static GUIStyle _successTitleStyle;
        private static Texture _successTexture;

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
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), DividerColor);
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

        /// <summary>
        /// Draws a smaller header for a group inside a section, for example one folder of the
        /// results, and returns the new expanded state.
        /// </summary>
        public static bool DrawGroupHeader(bool expanded, string label, int count, Color accent)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, GroupHeight, GUILayout.ExpandWidth(true));
            rect = new Rect(rect.x + 10f, rect.y, rect.width - 12f, rect.height);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(accent.r, accent.g, accent.b, 0.08f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2f, rect.height), new Color(accent.r, accent.g,
                    accent.b, 0.6f));
            }

            Rect foldoutRect = new(rect.x + 8f, rect.y, rect.width - 16f, rect.height);

            return EditorGUI.Foldout(foldoutRect, expanded, $"{label}  ({count})", true, GroupStyle);
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

        /// <summary>Draws the tinted strip and the baseline behind a table header.</summary>
        public static void DrawHeaderBackground(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, HeaderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), DividerColor);
        }

        /// <summary>Draws the centered green block shown when there is nothing left to fix.</summary>
        public static void DrawSuccess(string title, string subtitle)
        {
            EditorGUILayout.Space(SuccessGap);

            // Drawn through a label like in the project health overviews. A GUIStyle keeps the
            // image at its native size inside the box, while GUI.DrawTexture would stretch the
            // small built-in icon and blur it.
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(new GUIContent(SuccessTexture), GUILayout.Width(SuccessIconSize),
                    GUILayout.Height(SuccessIconSize));

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(SuccessTitleGap);
            GUILayout.Label(title, SuccessTitleStyle);
            GUILayout.Label(subtitle, SuccessSubtitleStyle);
            EditorGUILayout.Space(SuccessGap);
        }
    }
}