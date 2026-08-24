using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.CorePackage.Editor.EventBusInspector
{
    /// <summary>
    /// Sizes, colors and lazily built styles for the <see cref="EventBusWindow"/>, so the window
    /// itself stays about layout and interaction rather than appearance.
    /// <para>
    /// The shared editor look comes from <see cref="EditorPalette"/> and <see cref="EditorMetrics"/>;
    /// what stays here are the badge fills and the sizes only this window knows about.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Owns generated textures, so the window has to call <see cref="Dispose"/> when it closes.
    /// Everything is rebuilt when the editor skin changes, because the styles pin text colors picked
    /// for one skin and the textures are filled with one skin's colors.
    /// </remarks>
    internal sealed class EventBusStyles
    {
        private const float BadgeAlpha = 0.30f;
        private const int BadgeFontSize = 10;

        /// <summary>Horizontal gap between the badge column and its neighbors.</summary>
        internal const float BadgeGap = EditorMetrics.ItemGap;

        /// <summary>Width of the toolbar dropdown that picks between several buses.</summary>
        internal const float BusPopupWidth = 200f;

        /// <summary>Padding between the card edge and the first and last row.</summary>
        internal const int CardPadding = 4;

        /// <summary>Width the Handler column starts at before the user drags it.</summary>
        internal const float DefaultHandlerWidth = 180f;

        /// <summary>Width the Event column starts at before the user drags it.</summary>
        internal const float DefaultSubscriberWidth = 230f;

        private const float EmptyIconSize = 44f;

        /// <summary>Vertical gap between the parts of the empty state.</summary>
        internal const float EmptyLineGap = 6f;

        private const float GuideAlpha = 0.16f;

        /// <summary>Height of the column header strip.</summary>
        internal const float HeaderHeight = EditorMetrics.HeaderHeight;

        private const float HeaderTintDark = 0.07f;
        private const float HeaderTintLight = 0.06f;

        /// <summary>Gap between a column title and the sort arrow after it.</summary>
        internal const float HeaderArrowGap = 4f;

        /// <summary>Gap between a row icon and the name next to it.</summary>
        internal const float IconGap = 3f;

        /// <summary>Edge length of a small row icon.</summary>
        internal const float IconSize = 14f;

        /// <summary>
        /// Horizontal offset a subscriber row sits at under its event, and the width the expand
        /// arrow takes on the event row above it. They are one number on purpose: it is what lines
        /// an event name up with the column header and with the subscriber names below it.
        /// </summary>
        internal const float Indent = EditorMetrics.Indent;

        private const float LeakRowAlpha = 0.07f;

        /// <summary>Smallest width the badge column takes, so short badges still line up.</summary>
        internal const float MinBadgeWidth = 78f;

        /// <summary>Smallest height of the window.</summary>
        internal const float MinWindowHeight = 280f;

        /// <summary>Smallest width of the window, enough for every column plus the button.</summary>
        internal const float MinWindowWidth = 700f;

        private const float PingHoverAlpha = 0.45f;
        private const float PingRestAlpha = 0.17f;

        private const float NeutralBadgeAlpha = 0.12f;

        /// <summary>Outer margin around the table card.</summary>
        internal const float OuterMargin = 6f;

        /// <summary>Width of the button that selects and pings a subscriber.</summary>
        internal const float PingButtonWidth = 44f;

        /// <summary>Height of a single row.</summary>
        internal const float RowHeight = 24f;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        internal const float RowInset = EditorMetrics.RowInset;

        /// <summary>Width of the toolbar search field.</summary>
        internal const float SearchWidth = 190f;

        /// <summary>Height of the summary bar between the toolbar and the table.</summary>
        internal const float SummaryHeight = 24f;

        private const float SummaryPillAlpha = 0.26f;

        /// <summary>Width of the toolbar buttons.</summary>
        internal const float ToolbarButtonWidth = 62f;

        private readonly EditorTextureCache _textures = new();

        private bool _isBuilt;
        private bool _wasBuiltForProSkin;

        /// <summary>Fill of the badge carrying an event's subscriber count.</summary>
        internal static Color CountBadgeColor => WithAlpha(EditorPalette.Accent, BadgeAlpha);

        /// <summary>Fill of the badge on a subscription whose object was destroyed.</summary>
        internal static Color DestroyedBadgeColor => WithAlpha(EditorPalette.Danger, BadgeAlpha);

        /// <summary>Background of an event row, which reads as a header for the rows under it.</summary>
        internal static Color GroupColor => EditorPalette.Tint(HeaderTintDark, HeaderTintLight);

        /// <summary>The vertical line that ties a subscriber row back to its event.</summary>
        internal static Color GuideColor => EditorPalette.Tint(GuideAlpha);

        /// <summary>Background of the column header strip.</summary>
        internal static Color HeaderColor => EditorPalette.Tint(HeaderTintDark, HeaderTintLight);

        /// <summary>Fill behind the ping button while the mouse is on it.</summary>
        internal static Color PingHoverColor => WithAlpha(EditorPalette.Accent, PingHoverAlpha);

        /// <summary>
        /// Fill behind the ping button at rest. The accent at low opacity, which lands on a muted
        /// slate blue over the card on either skin rather than needing a color of its own.
        /// </summary>
        internal static Color PingRestColor => WithAlpha(EditorPalette.Accent, PingRestAlpha);

        /// <summary>Tint laid over a row that holds or contains a leaked subscription.</summary>
        internal static Color LeakRowColor => WithAlpha(EditorPalette.Danger, LeakRowAlpha);

        /// <summary>Fill of the badge on a subscription whose object is still alive.</summary>
        internal static Color LiveBadgeColor => WithAlpha(EditorPalette.Success, BadgeAlpha);

        /// <summary>Fill of the badge on a subscription with no Unity object behind it.</summary>
        internal static Color NeutralBadgeColor => EditorPalette.Tint(NeutralBadgeAlpha);

        /// <summary>Fill of the summary pill when nothing leaked.</summary>
        internal static Color SummaryOkColor => WithAlpha(EditorPalette.Success, SummaryPillAlpha);

        /// <summary>Fill of the summary pill when at least one subscription leaked.</summary>
        internal static Color SummaryProblemColor => WithAlpha(EditorPalette.Danger, SummaryPillAlpha);

        /// <summary>Centered label inside a pill.</summary>
        internal GUIStyle Badge { get; private set; }

        /// <summary>Rounded card the table sits in.</summary>
        internal GUIStyle Card { get; private set; }

        /// <summary>Dimmed secondary column, clipped rather than pushing the other columns around.</summary>
        internal GUIStyle Detail { get; private set; }

        /// <summary>Centered hint under the empty state message.</summary>
        internal GUIStyle EmptyHint { get; private set; }

        /// <summary>Centered headline of the empty state.</summary>
        internal GUIStyle EmptyTitle { get; private set; }

        /// <summary>The event type on a group row.</summary>
        internal GUIStyle Group { get; private set; }

        /// <summary>Title above a column.</summary>
        internal GUIStyle Header { get; private set; }

        /// <summary>The subscribing type on a handler row.</summary>
        internal GUIStyle Name { get; private set; }

        /// <summary>
        /// A rounded white background, drawn tinted through <see cref="GUI.color"/> so one texture
        /// serves every pill color instead of one texture per state.
        /// </summary>
        internal GUIStyle PillBackground { get; private set; }

        /// <summary>Label of the ping button at rest, when the button has no fill behind it.</summary>
        internal GUIStyle Ping { get; private set; }

        /// <summary>Label of the ping button while the mouse is on it.</summary>
        internal GUIStyle PingHot { get; private set; }

        /// <summary>The line under the toolbar that says what the table is showing.</summary>
        internal GUIStyle Summary { get; private set; }

        /// <summary>The rectangle the empty state icon is drawn in, centered above the message.</summary>
        /// <param name="area">The area the empty state fills.</param>
        /// <returns>The icon rectangle.</returns>
        internal static Rect EmptyIconRect(Rect area) => new(area.center.x - EmptyIconSize * 0.5f,
            area.center.y - EmptyIconSize, EmptyIconSize, EmptyIconSize);

        /// <summary>
        /// Builds the styles once, and again after a skin change. Must run inside a GUI callback,
        /// because <see cref="EditorStyles"/> is not valid before that.
        /// </summary>
        internal void EnsureBuilt()
        {
            if (_isBuilt && _wasBuiltForProSkin == EditorGUIUtility.isProSkin)
                return;

            _textures.Release();
            Build();

            _isBuilt = true;
            _wasBuiltForProSkin = EditorGUIUtility.isProSkin;
        }

        /// <summary>Destroys the generated textures. Call when the owning window closes.</summary>
        internal void Dispose()
        {
            _textures.Release();

            _isBuilt = false;
        }

        private static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);

        private void Build()
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

            Card.normal.background = _textures.Rounded(EditorPalette.Card, EditorMetrics.CardCornerRadius);

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

            Group = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis,
                fontStyle = FontStyle.Bold
            }, EditorPalette.Text);

            Header = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            Name = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis
            }, EditorPalette.Text);

            PillBackground = new GUIStyle
            {
                border = EditorStyleUtility.UniformPadding(EditorMetrics.PillCornerRadius)
            };

            PillBackground.normal.background = _textures.Rounded(Color.white, EditorMetrics.PillCornerRadius);

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