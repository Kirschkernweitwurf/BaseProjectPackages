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
    /// Everything here is what the Base list windows turned out to agree on down to the value. What a
    /// window keeps for itself are the widths of its own columns, its minimum size, and the fills of
    /// the badges only it has a meaning for. Those go in a subclass, which may also override
    /// <see cref="Build"/> and call the base first to add styles of its own.
    /// <para>
    /// Every number and color comes from the active theme, so a user retunes the whole family of list
    /// windows from the Editor UI Theme project settings page.
    /// </para>
    /// <para>
    /// Building and releasing are inherited from <see cref="EditorStyleSet"/>: call
    /// <c>EnsureBuilt</c> at the top of <c>OnGUI</c> and <c>Dispose</c> from <c>OnDisable</c>.
    /// </para>
    /// </remarks>
    public class EditorTableStyles : EditorStyleSet
    {
        /// <summary>Opacity a palette color is laid on a badge with, so the text stays readable.</summary>
        public static float BadgeAlpha => EditorThemeProvider.Table.BadgeAlpha;

        /// <summary>Horizontal gap between the badge column and its neighbors.</summary>
        public static float BadgeGap => EditorMetrics.ItemGap;

        /// <summary>Padding between the card edge and the first and last row.</summary>
        public static int CardPadding => EditorThemeProvider.Table.CardPadding;

        /// <summary>Edge length of the large icon above an empty state message.</summary>
        public static float EmptyIconSize => EditorThemeProvider.Table.EmptyIconSize;

        /// <summary>Vertical gap between the parts of the empty state.</summary>
        public static float EmptyLineGap => EditorThemeProvider.Table.EmptyLineGap;

        /// <summary>Gap between a column title and the sort arrow after it.</summary>
        public static float HeaderArrowGap => EditorThemeProvider.Table.HeaderArrowGap;

        /// <summary>Height of the column header strip.</summary>
        public static float HeaderHeight => EditorMetrics.HeaderHeight;

        /// <summary>Gap between a row icon and the name next to it.</summary>
        public static float IconGap => EditorThemeProvider.Table.IconGap;

        /// <summary>Edge length of a small row icon.</summary>
        public static float IconSize => EditorThemeProvider.Table.IconSize;

        /// <summary>Smallest width the badge column takes, so short badges still line up.</summary>
        public static float MinBadgeWidth => EditorThemeProvider.Table.MinBadgeWidth;

        /// <summary>Outer margin around the table card.</summary>
        public static float OuterMargin => EditorThemeProvider.Table.OuterMargin;

        /// <summary>Width of the button that selects and pings a row's object.</summary>
        public static float PingButtonWidth => EditorThemeProvider.Table.PingButtonWidth;

        /// <summary>Height of a single row. Taller than a plain list row, because rows carry badges.</summary>
        public static float RowHeight => EditorThemeProvider.Table.RowHeight;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public static float RowInset => EditorMetrics.RowInset;

        /// <summary>Width of the toolbar search field.</summary>
        public static float SearchWidth => EditorThemeProvider.Table.SearchWidth;

        /// <summary>Height of the summary bar between the toolbar and the table.</summary>
        public static float SummaryHeight => EditorThemeProvider.Table.SummaryHeight;

        /// <summary>Width of the toolbar buttons.</summary>
        public static float ToolbarButtonWidth => EditorThemeProvider.Table.ToolbarButtonWidth;

        /// <summary>Fill of the badge on a row that reports a problem.</summary>
        public static Color DangerBadgeColor => EditorPalette.WithAlpha(EditorPalette.Danger, BadgeAlpha);

        /// <summary>Background of the column header strip.</summary>
        public static Color HeaderColor => EditorPalette.Tint(EditorThemeProvider.Table.HeaderTintDark,
            EditorThemeProvider.Table.HeaderTintLight);

        /// <summary>Fill of a badge that only carries a number or a label, with no state behind it.</summary>
        public static Color NeutralBadgeColor => EditorPalette.Tint(EditorThemeProvider.Table.NeutralBadgeAlpha);

        /// <summary>Fill of the badge on a row that is fine.</summary>
        public static Color OkBadgeColor => EditorPalette.WithAlpha(EditorPalette.Success, BadgeAlpha);

        /// <summary>Fill behind the ping button while the mouse is on it.</summary>
        public static Color PingHoverColor => EditorPalette.WithAlpha(EditorPalette.Accent,
            EditorThemeProvider.Table.PingHoverAlpha);

        /// <summary>
        /// Fill behind the ping button at rest. The accent at low opacity, which lands on a muted
        /// slate blue over the card on either editor theme rather than needing a color of its own.
        /// </summary>
        public static Color PingRestColor => EditorPalette.WithAlpha(EditorPalette.Accent,
            EditorThemeProvider.Table.PingRestAlpha);

        /// <summary>Fill of the summary pill when nothing is wrong.</summary>
        public static Color SummaryOkColor => EditorPalette.WithAlpha(EditorPalette.Success,
            EditorThemeProvider.Table.SummaryPillAlpha);

        /// <summary>Fill of the summary pill when at least one row reports a problem.</summary>
        public static Color SummaryProblemColor => EditorPalette.WithAlpha(EditorPalette.Danger,
            EditorThemeProvider.Table.SummaryPillAlpha);

        /// <summary>Fill of the badge on a row that is worth a second look but is not broken.</summary>
        public static Color WarningBadgeColor => EditorPalette.WithAlpha(EditorPalette.Warning, BadgeAlpha);

        private static int BadgeFontSize => EditorThemeProvider.Table.BadgeFontSize;

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

        /// <summary>Filled accent button for the one action a window is mostly opened for.</summary>
        public GUIStyle PrimaryButton { get; private set; }

        /// <summary>Rounded fill behind a search box.</summary>
        public GUIStyle SearchField { get; private set; }

        /// <summary>
        /// Text typed into a search box, with no background of its own so the fill behind it and the
        /// icon in front of it can be placed by hand.
        /// </summary>
        public GUIStyle SearchText { get; private set; }

        /// <summary>Filled muted button for every action next to the primary one.</summary>
        public GUIStyle SecondaryButton { get; private set; }

        /// <summary>The line under the toolbar that says what the table is showing.</summary>
        public GUIStyle Summary { get; private set; }

        /// <summary>
        /// A badge fill mixed from any color at the theme's badge opacity, for a window whose
        /// states the shared fills have no name for.
        /// </summary>
        /// <param name="color">The color the badge stands for.</param>
        /// <returns>The fill to draw the badge with.</returns>
        public static Color BadgeFill(Color color) => EditorPalette.WithAlpha(color, BadgeAlpha);

        /// <summary>The rectangle the empty state icon is drawn in, centered above the message.</summary>
        /// <param name="area">The area the empty state fills.</param>
        /// <returns>The icon rectangle.</returns>
        public static Rect EmptyIconRect(Rect area) => new(area.center.x - EmptyIconSize * 0.5f,
            area.center.y - EmptyIconSize, EmptyIconSize, EmptyIconSize);

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

            PrimaryButton = EditorStyleUtility.BuildFilledButton(Textures, EditorPalette.Accent,
                EditorPalette.AccentText, FontStyle.Bold, EditorMetrics.CardCornerRadius);

            SearchField = new GUIStyle
            {
                border = EditorStyleUtility.UniformPadding(EditorMetrics.PillCornerRadius)
            };

            SearchField.normal.background = Textures.Rounded(EditorPalette.Field,
                EditorMetrics.PillCornerRadius);

            // Clipped rather than ellipsized, because a caret sitting past the right edge of an
            // ellipsis has nowhere sensible to be drawn.
            SearchText = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = EditorStyleUtility.HorizontalPadding(0)
            }, EditorPalette.Text);

            SecondaryButton = EditorStyleUtility.BuildFilledButton(Textures, EditorPalette.Secondary,
                EditorPalette.SecondaryText, FontStyle.Normal, EditorMetrics.CardCornerRadius);

            Summary = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);
        }
    }
}