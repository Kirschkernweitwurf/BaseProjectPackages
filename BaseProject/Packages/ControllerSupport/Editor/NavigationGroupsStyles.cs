using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupportPackage.Editor
{
    /// <summary>
    /// Colors, layout metrics and lazily built styles for the <see cref="NavigationGroupsWindow"/>.
    /// Keeps the window itself about layout and interaction instead of appearance.
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/>, <see cref="EditorMetrics"/> and
    /// <see cref="EditorTableStyles"/>, so this window follows the theme the project is set to. What
    /// stays here are the widths only this window knows about and the two badge hues that stand for
    /// something no shared name covers.
    /// </para>
    /// </summary>
    internal static class NavigationGroupsStyles
    {
        private const int BadgeFontSize = 10;

        /// <summary>Height of a row button.</summary>
        public const float ButtonHeight = 18f;

        /// <summary>Width of the "Go to" and "Rebuild" buttons.</summary>
        public const float ButtonWidth = 56f;

        /// <summary>Width of the "Fix" button.</summary>
        public const float FixButtonWidth = 40f;

        private const float IssueRowAlpha = 0.06f;

        /// <summary>Smallest width any badge column takes.</summary>
        public const float MinBadgeWidth = 64f;

        /// <summary>Smallest height of the window.</summary>
        public const float MinWindowHeight = 200f;

        /// <summary>Smallest width of the window, enough for every column plus the buttons.</summary>
        public const float MinWindowWidth = 680f;

        /// <summary>Height of a group row.</summary>
        public const float RowHeight = 26f;

        /// <summary>Width of the toolbar's "Refresh" button.</summary>
        public const float ToolbarButtonWidth = 60f;

        private static readonly EditorStyleWatch Watch = new();

        // The two hues the palette has no name for: a count is not a state, and a focus priority is
        // not the amber the palette reserves for a link or an override.
        private static readonly Color ElementsHue = new(0.70f, 0.45f, 0.95f);
        private static readonly Color PriorityHue = new(0.35f, 0.55f, 0.95f);

        private static GUIStyle _badge;
        private static GUIStyle _header;
        private static GUIStyle _name;

        /// <summary>Horizontal gap between two badges or buttons.</summary>
        public static float BadgeGap => EditorMetrics.TightGap;

        /// <summary>Height of a badge.</summary>
        public static float BadgeHeight => EditorMetrics.BadgeHeight;

        /// <summary>Height of the column header strip.</summary>
        public static float HeaderHeight => EditorMetrics.HeaderHeight;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public static float RowPadding => EditorMetrics.RowInset;

        /// <summary>Thickness of the line below the header and every row.</summary>
        public static float SeparatorThickness => EditorMetrics.SeparatorThickness;

        /// <summary>Centered mini label used inside a badge.</summary>
        public static GUIStyle Badge
        {
            get
            {
                EnsureBuilt();

                return _badge;
            }
        }

        /// <summary>Centered bold mini label used in the column header.</summary>
        public static GUIStyle Header
        {
            get
            {
                EnsureBuilt();

                return _header;
            }
        }

        /// <summary>Bold label used for the group name.</summary>
        public static GUIStyle Name
        {
            get
            {
                EnsureBuilt();

                return _name;
            }
        }

        /// <summary>Background of the column header strip.</summary>
        public static Color HeaderColor => EditorTableStyles.HeaderColor;

        /// <summary>Tint of the row under the mouse.</summary>
        public static Color HoverColor => EditorPalette.Hover;

        /// <summary>Color of the line below the header and every row.</summary>
        public static Color SeparatorColor => EditorPalette.Separator;

        /// <summary>Tint of every second row.</summary>
        public static Color StripeColor => EditorPalette.Stripe;

        /// <summary>Badge color of the element count.</summary>
        public static Color ElementsBadgeColor => EditorTableStyles.BadgeFill(ElementsHue);

        /// <summary>Badge color of a group without any elements.</summary>
        public static Color EmptyBadgeColor => EditorTableStyles.WarningBadgeColor;

        /// <summary>Tint of a row that breaks a menu rule.</summary>
        public static Color IssueRowColor => EditorPalette.WithAlpha(EditorPalette.Warning, IssueRowAlpha);

        /// <summary>Badge color of a group that sits on a menu.</summary>
        public static Color MenuBadgeColor => EditorTableStyles.OkBadgeColor;

        /// <summary>Badge color of a group without a menu.</summary>
        public static Color NoMenuBadgeColor => EditorTableStyles.NeutralBadgeColor;

        /// <summary>Badge color of the focus priority.</summary>
        public static Color PriorityBadgeColor => EditorTableStyles.BadgeFill(PriorityHue);

        /// <summary>Badge color of the scene name.</summary>
        public static Color SceneBadgeColor => EditorTableStyles.NeutralBadgeColor;

        /// <summary>Badge color of a value that breaks a menu rule.</summary>
        public static Color WarningBadgeColor => EditorTableStyles.WarningBadgeColor;

        /// <summary>Width a badge needs for the given text, never below the shared minimum.</summary>
        /// <param name="text">The badge text to measure.</param>
        /// <returns>The width to lay the badge out with.</returns>
        public static float MeasureBadge(string text) => EditorRows.MeasureBadge(text, Badge, MinBadgeWidth);

        // The styles pin colors picked for one editor theme, so they are dropped whenever the editor theme or the
        // active theme moves. Reached through the properties rather than from a window callback,
        // because a static class has no lifetime a window could hang the rebuild off.
        private static void EnsureBuilt()
        {
            if (!Watch.IsStale)
                return;

            _badge = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = BadgeFontSize
            };

            _header = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            _name = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            Watch.MarkFresh();
        }
    }
}