using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The styles, sizes and fills of a list window: a rounded card holding striped rows, each with a
    /// name, a dimmed detail column, a colored badge and a ping button, under a toolbar and a summary
    /// line, with a centered message when there is nothing to list.
    /// </summary>
    /// <remarks>
    /// Everything here is what two such windows turned out to agree on down to the value. What a
    /// window keeps for itself are the widths of its own columns, its minimum size, and the fills of
    /// the badges only it has a meaning for. Those go in a subclass, which may also override
    /// <see cref="Build"/> and call the base first to add styles of its own.
    /// <para>
    /// Building and releasing are inherited from <see cref="EditorStyleSet"/>: call
    /// <c>EnsureBuilt</c> at the top of <c>OnGUI</c> and <c>Dispose</c> from <c>OnDisable</c>.
    /// </para>
    /// </remarks>
    public class EditorTableStyles : EditorStyleSet
    {
        /// <summary>Opacity a palette color is laid on a badge with, so the text stays readable.</summary>
        protected const float BadgeAlpha = 0.30f;

        private const int BadgeFontSize = 10;

        /// <summary>Horizontal gap between the badge column and its neighbors.</summary>
        public const float BadgeGap = EditorMetrics.ItemGap;

        /// <summary>Padding between the card edge and the first and last row.</summary>
        public const int CardPadding = 4;

        private const float EmptyIconSize = 44f;

        /// <summary>Vertical gap between the parts of the empty state.</summary>
        public const float EmptyLineGap = 6f;

        /// <summary>Gap between a column title and the sort arrow after it.</summary>
        public const float HeaderArrowGap = 4f;

        /// <summary>Height of the column header strip.</summary>
        public const float HeaderHeight = EditorMetrics.HeaderHeight;

        private const float HeaderTintDark = 0.07f;
        private const float HeaderTintLight = 0.06f;

        /// <summary>Gap between a row icon and the name next to it.</summary>
        public const float IconGap = 3f;

        /// <summary>Edge length of a small row icon.</summary>
        public const float IconSize = 14f;

        /// <summary>Smallest width the badge column takes, so short badges still line up.</summary>
        public const float MinBadgeWidth = 78f;

        private const float NeutralBadgeAlpha = 0.12f;

        /// <summary>Outer margin around the table card.</summary>
        public const float OuterMargin = 6f;

        /// <summary>Width of the button that selects and pings a row's object.</summary>
        public const float PingButtonWidth = 44f;

        private const float PingHoverAlpha = 0.45f;
        private const float PingRestAlpha = 0.17f;

        /// <summary>Height of a single row. Taller than a plain list row, because rows carry badges.</summary>
        public const float RowHeight = 24f;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public const float RowInset = EditorMetrics.RowInset;

        /// <summary>Width of the toolbar search field.</summary>
        public const float SearchWidth = 190f;

        /// <summary>Height of the summary bar between the toolbar and the table.</summary>
        public const float SummaryHeight = 24f;

        private const float SummaryPillAlpha = 0.26f;

        /// <summary>Width of the toolbar buttons.</summary>
        public const float ToolbarButtonWidth = 62f;

        /// <summary>Fill of the badge on a row that reports a problem.</summary>
        public static Color DangerBadgeColor => WithAlpha(EditorPalette.Danger, BadgeAlpha);

        /// <summary>Background of the column header strip.</summary>
        public static Color HeaderColor => EditorPalette.Tint(HeaderTintDark, HeaderTintLight);

        /// <summary>Fill of a badge that only carries a number or a label, with no state behind it.</summary>
        public static Color NeutralBadgeColor => EditorPalette.Tint(NeutralBadgeAlpha);

        /// <summary>Fill of the badge on a row that is fine.</summary>
        public static Color OkBadgeColor => WithAlpha(EditorPalette.Success, BadgeAlpha);

        /// <summary>Fill behind the ping button while the mouse is on it.</summary>
        public static Color PingHoverColor => WithAlpha(EditorPalette.Accent, PingHoverAlpha);

        /// <summary>
        /// Fill behind the ping button at rest. The accent at low opacity, which lands on a muted
        /// slate blue over the card on either skin rather than needing a color of its own.
        /// </summary>
        public static Color PingRestColor => WithAlpha(EditorPalette.Accent, PingRestAlpha);

        /// <summary>Fill of the summary pill when nothing is wrong.</summary>
        public static Color SummaryOkColor => WithAlpha(EditorPalette.Success, SummaryPillAlpha);

        /// <summary>Fill of the summary pill when at least one row reports a problem.</summary>
        public static Color SummaryProblemColor => WithAlpha(EditorPalette.Danger, SummaryPillAlpha);

        /// <summary>Fill of the badge on a row that is worth a second look but is not broken.</summary>
        public static Color WarningBadgeColor => WithAlpha(EditorPalette.Warning, BadgeAlpha);

        /// <summary>Centered label inside a pill.</summary>
        public GUIStyle Badge { get; private set; }

        /// <summary>Rounded card the table sits in.</summary>
        public GUIStyle Card { get; private set; }

        /// <summary>Dimmed secondary column, clipped rather than pushing the other columns around.</summary>
        public GUIStyle Detail { get; private set; }

        /// <summary>Centered hint under the empty state message.</summary>
        public GUIStyle EmptyHint { get; private set; }

        /// <summary>Centered headline of the empty state.</summary>
        public GUIStyle EmptyTitle { get; private set; }

        /// <summary>Title above a column.</summary>
        public GUIStyle Header { get; private set; }

        /// <summary>The name a row is read by, clipped when the column is too narrow for it.</summary>
        public GUIStyle Name { get; private set; }

        /// <summary>
        /// The same name in bold, for the column the eye should land on first and for a row that
        /// heads the rows under it.
        /// </summary>
        public GUIStyle NameBold { get; private set; }

        /// <summary>
        /// A rounded white background, drawn tinted through <see cref="GUI.color"/> so one texture
        /// serves every pill color instead of one texture per state.
        /// </summary>
        public GUIStyle PillBackground { get; private set; }

        /// <summary>Label of the ping button at rest, when the button has no fill behind it.</summary>
        public GUIStyle Ping { get; private set; }

        /// <summary>Label of the ping button while the mouse is on it.</summary>
        public GUIStyle PingHot { get; private set; }

        /// <summary>The line under the toolbar that says what the table is showing.</summary>
        public GUIStyle Summary { get; private set; }

        /// <summary>The rectangle the empty state icon is drawn in, centered above the message.</summary>
        /// <param name="area">The area the empty state fills.</param>
        /// <returns>The icon rectangle.</returns>
        public static Rect EmptyIconRect(Rect area) => new(area.center.x - EmptyIconSize * 0.5f,
            area.center.y - EmptyIconSize, EmptyIconSize, EmptyIconSize);

        /// <summary>The same color at a different opacity, which is how every badge fill is made.</summary>
        /// <param name="color">The color from the palette.</param>
        /// <param name="alpha">The opacity to lay it on with.</param>
        /// <returns>The badge fill.</returns>
        protected static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        /// <inheritdoc/>
        protected override void Build()
        {
            Badge = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = BadgeFontSize
            }, EditorPalette.Text);

            Card = new GUIStyle
            {
                border = EditorStyleUtility.UniformPadding(EditorMetrics.CardCornerRadius),
                padding = new RectOffset(0, 0, CardPadding, CardPadding)
            };

            Card.normal.background = Textures.Rounded(EditorPalette.Card, EditorMetrics.CardCornerRadius);

            Detail = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis
            }, EditorPalette.DimText);

            EmptyHint = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            }, EditorPalette.DimText);

            EmptyTitle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true
            }, EditorPalette.DimText);

            Header = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            Name = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis
            }, EditorPalette.Text);

            NameBold = EditorStyleUtility.PinTextColor(new GUIStyle(Name)
            {
                fontStyle = FontStyle.Bold
            }, EditorPalette.Text);

            PillBackground = new GUIStyle
            {
                border = EditorStyleUtility.UniformPadding(EditorMetrics.PillCornerRadius)
            };

            PillBackground.normal.background = Textures.Rounded(Color.white, EditorMetrics.PillCornerRadius);

            // Text only, with the fill behind it drawn by hand. A GUIStyle resolves its own hover
            // state through a background it does not have at rest, so the fill never appeared.
            Ping = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = BadgeFontSize,
                padding = EditorStyleUtility.HorizontalPadding(0)
            }, EditorPalette.DimText);

            PingHot = EditorStyleUtility.PinTextColor(new GUIStyle(Ping)
            {
                fontStyle = FontStyle.Bold
            }, EditorPalette.Text);

            Summary = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);
        }
    }
}