using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// Colors, layout metrics and lazily built styles for the <see cref="NavigationGroupsWindow"/>.
    /// Keeps the window itself about layout and interaction instead of appearance.
    /// </summary>
    internal static class NavigationGroupsStyles
    {
        /// <summary>Horizontal gap between two badges or buttons.</summary>
        public const float BadgeGap = 4f;

        /// <summary>Height of a badge.</summary>
        public const float BadgeHeight = 16f;

        /// <summary>Horizontal padding added to the measured text of a badge.</summary>
        public const float BadgePadding = 14f;

        /// <summary>Height of a row button.</summary>
        public const float ButtonHeight = 18f;

        /// <summary>Width of the "Go to" and "Rebuild" buttons.</summary>
        public const float ButtonWidth = 56f;

        /// <summary>Width of the "Fix" button.</summary>
        public const float FixButtonWidth = 40f;

        /// <summary>Height of the column header strip.</summary>
        public const float HeaderHeight = 20f;

        /// <summary>Smallest width any badge column takes.</summary>
        public const float MinBadgeWidth = 64f;

        /// <summary>Smallest height of the window.</summary>
        public const float MinWindowHeight = 200f;

        /// <summary>Smallest width of the window, enough for every column plus the buttons.</summary>
        public const float MinWindowWidth = 680f;

        /// <summary>Height of a group row.</summary>
        public const float RowHeight = 26f;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public const float RowPadding = 6f;

        /// <summary>Thickness of the line below the header and every row.</summary>
        public const float SeparatorThickness = 1f;

        /// <summary>Width of the toolbar's "Refresh" button.</summary>
        public const float ToolbarButtonWidth = 60f;
        private const int BadgeFontSize = 10;

        /// <summary>Centered mini label used inside a badge.</summary>
        public static GUIStyle Badge => _badge ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = BadgeFontSize
        };

        /// <summary>Centered bold mini label used in the column header.</summary>
        public static GUIStyle Header => _header ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        /// <summary>Bold label used for the group name.</summary>
        public static GUIStyle Name => _name ??= new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };

        /// <summary>Badge color of the element count.</summary>
        public static readonly Color ElementsBadgeColor = new(0.7f, 0.45f, 0.95f, 0.32f);

        /// <summary>Badge color of a group without any elements.</summary>
        public static readonly Color EmptyBadgeColor = new(0.95f, 0.55f, 0.2f, 0.4f);

        /// <summary>Background of the column header strip.</summary>
        public static readonly Color HeaderColor = new(0f, 0f, 0f, 0.18f);

        /// <summary>Tint of the row under the mouse.</summary>
        public static readonly Color HoverColor = new(1f, 1f, 1f, 0.05f);

        /// <summary>Tint of a row that breaks a menu rule.</summary>
        public static readonly Color IssueRowColor = new(0.95f, 0.45f, 0.2f, 0.06f);

        /// <summary>Badge color of a group that sits on a menu.</summary>
        public static readonly Color MenuBadgeColor = new(0.3f, 0.7f, 0.4f, 0.32f);

        /// <summary>Badge color of a group without a menu.</summary>
        public static readonly Color NoMenuBadgeColor = new(0.5f, 0.5f, 0.5f, 0.12f);

        /// <summary>Badge color of the focus priority.</summary>
        public static readonly Color PriorityBadgeColor = new(0.35f, 0.55f, 0.95f, 0.32f);

        /// <summary>Badge color of the scene name.</summary>
        public static readonly Color SceneBadgeColor = new(0.5f, 0.5f, 0.5f, 0.28f);

        /// <summary>Color of the line below the header and every row.</summary>
        public static readonly Color SeparatorColor = new(0f, 0f, 0f, 0.25f);

        /// <summary>Tint of every second row.</summary>
        public static readonly Color StripeColor = new(1f, 1f, 1f, 0.02f);

        /// <summary>Badge color of a value that breaks a menu rule.</summary>
        public static readonly Color WarningBadgeColor = new(0.95f, 0.55f, 0.2f, 0.45f);

        private static GUIStyle _badge;
        private static GUIStyle _header;
        private static GUIStyle _name;

        /// <summary>Width a badge needs for the given text, never below the shared minimum.</summary>
        public static float MeasureBadge(string text)
            => Mathf.Max(MinBadgeWidth, Badge.CalcSize(new GUIContent(text)).x + BadgePadding);
    }
}