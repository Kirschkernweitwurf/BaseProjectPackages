using System;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The numbers and opacities that describe a list window specifically: the card it sits in, the
    /// badges its rows carry, the ping button, the toolbar and the empty state. Read through
    /// <see cref="EditorTableStyles"/>.
    /// </summary>
    /// <remarks>
    /// Separate from <see cref="EditorThemeMetrics"/> because these only mean something to a window
    /// built as a table, while the metrics apply to any window at all.
    /// </remarks>
    [Serializable]
    public sealed class EditorThemeTable
    {
        [SerializeField] [Range(0f, 1f)] private float badgeAlpha;
        [SerializeField] [Min(1)] private int badgeFontSize;
        [SerializeField] [Min(0)] private int cardPadding;
        [SerializeField] [Min(0f)] private float emptyIconSize;
        [SerializeField] [Min(0f)] private float emptyLineGap;
        [SerializeField] [Min(0f)] private float headerArrowGap;
        [SerializeField] [Range(0f, 1f)] private float headerTintDark;
        [SerializeField] [Range(0f, 1f)] private float headerTintLight;
        [SerializeField] [Min(0f)] private float iconGap;
        [SerializeField] [Min(0f)] private float iconSize;
        [SerializeField] [Min(0f)] private float minBadgeWidth;
        [SerializeField] [Range(0f, 1f)] private float neutralBadgeAlpha;
        [SerializeField] [Min(0f)] private float outerMargin;
        [SerializeField] [Min(0f)] private float pingButtonWidth;
        [SerializeField] [Range(0f, 1f)] private float pingHoverAlpha;
        [SerializeField] [Range(0f, 1f)] private float pingRestAlpha;
        [SerializeField] [Min(1f)] private float rowHeight;
        [SerializeField] [Min(0f)] private float searchWidth;
        [SerializeField] [Min(0f)] private float summaryHeight;
        [SerializeField] [Range(0f, 1f)] private float summaryPillAlpha;
        [SerializeField] [Min(0f)] private float toolbarButtonWidth;

        /// <summary>Opacity a palette color is laid on a badge with, so the text stays readable.</summary>
        public float BadgeAlpha => badgeAlpha;

        /// <summary>Font size of a badge and of the ping button label.</summary>
        public int BadgeFontSize => Mathf.Max(1, badgeFontSize);

        /// <summary>Padding between the card edge and the first and last row.</summary>
        public int CardPadding => Mathf.Max(0, cardPadding);

        /// <summary>Edge length of the large icon above the empty state message.</summary>
        public float EmptyIconSize => emptyIconSize;

        /// <summary>Vertical gap between the parts of the empty state.</summary>
        public float EmptyLineGap => emptyLineGap;

        /// <summary>Gap between a column title and the sort arrow after it.</summary>
        public float HeaderArrowGap => headerArrowGap;

        /// <summary>Opacity of the column header strip on the dark skin.</summary>
        public float HeaderTintDark => headerTintDark;

        /// <summary>Opacity of the column header strip on the light skin.</summary>
        public float HeaderTintLight => headerTintLight;

        /// <summary>Gap between a row icon and the name next to it.</summary>
        public float IconGap => iconGap;

        /// <summary>Edge length of a small row icon.</summary>
        public float IconSize => iconSize;

        /// <summary>Smallest width the badge column takes, so short badges still line up.</summary>
        public float MinBadgeWidth => minBadgeWidth;

        /// <summary>Opacity of a badge that carries no state, only a number or a label.</summary>
        public float NeutralBadgeAlpha => neutralBadgeAlpha;

        /// <summary>Outer margin around the table card.</summary>
        public float OuterMargin => outerMargin;

        /// <summary>Width of the button that selects and pings a row's object.</summary>
        public float PingButtonWidth => pingButtonWidth;

        /// <summary>Opacity of the fill behind the ping button while the mouse is on it.</summary>
        public float PingHoverAlpha => pingHoverAlpha;

        /// <summary>Opacity of the fill behind the ping button at rest.</summary>
        public float PingRestAlpha => pingRestAlpha;

        /// <summary>Height of a single row. Taller than a plain list row, because rows carry badges.</summary>
        public float RowHeight => Mathf.Max(1f, rowHeight);

        /// <summary>Width of the toolbar search field.</summary>
        public float SearchWidth => searchWidth;

        /// <summary>Height of the summary bar between the toolbar and the table.</summary>
        public float SummaryHeight => summaryHeight;

        /// <summary>Opacity of the summary pill.</summary>
        public float SummaryPillAlpha => summaryPillAlpha;

        /// <summary>Width of the toolbar buttons.</summary>
        public float ToolbarButtonWidth => toolbarButtonWidth;

        /// <summary>Creates an empty set. Required by the serializer and by the inspector.</summary>
        public EditorThemeTable() { }

        /// <summary>Creates a full set of list window metrics.</summary>
        /// <param name="badgeAlpha">Opacity a palette color is laid on a badge with.</param>
        /// <param name="badgeFontSize">Font size of a badge and of the ping button label.</param>
        /// <param name="cardPadding">Padding between the card edge and the first and last row.</param>
        /// <param name="emptyIconSize">Edge length of the large empty state icon.</param>
        /// <param name="emptyLineGap">Vertical gap between the parts of the empty state.</param>
        /// <param name="headerArrowGap">Gap between a column title and the sort arrow after it.</param>
        /// <param name="headerTintDark">Opacity of the column header strip on the dark skin.</param>
        /// <param name="headerTintLight">Opacity of the column header strip on the light skin.</param>
        /// <param name="iconGap">Gap between a row icon and the name next to it.</param>
        /// <param name="iconSize">Edge length of a small row icon.</param>
        /// <param name="minBadgeWidth">Smallest width the badge column takes.</param>
        /// <param name="neutralBadgeAlpha">Opacity of a badge that carries no state.</param>
        /// <param name="outerMargin">Outer margin around the table card.</param>
        /// <param name="pingButtonWidth">Width of the ping button.</param>
        /// <param name="pingHoverAlpha">Opacity behind the ping button while hovered.</param>
        /// <param name="pingRestAlpha">Opacity behind the ping button at rest.</param>
        /// <param name="rowHeight">Height of a single row.</param>
        /// <param name="searchWidth">Width of the toolbar search field.</param>
        /// <param name="summaryHeight">Height of the summary bar.</param>
        /// <param name="summaryPillAlpha">Opacity of the summary pill.</param>
        /// <param name="toolbarButtonWidth">Width of the toolbar buttons.</param>
        public EditorThemeTable(float badgeAlpha, int badgeFontSize, int cardPadding, float emptyIconSize,
            float emptyLineGap, float headerArrowGap, float headerTintDark, float headerTintLight, float iconGap,
            float iconSize, float minBadgeWidth, float neutralBadgeAlpha, float outerMargin, float pingButtonWidth,
            float pingHoverAlpha, float pingRestAlpha, float rowHeight, float searchWidth, float summaryHeight,
            float summaryPillAlpha, float toolbarButtonWidth)
        {
            this.badgeAlpha = badgeAlpha;
            this.badgeFontSize = badgeFontSize;
            this.cardPadding = cardPadding;
            this.emptyIconSize = emptyIconSize;
            this.emptyLineGap = emptyLineGap;
            this.headerArrowGap = headerArrowGap;
            this.headerTintDark = headerTintDark;
            this.headerTintLight = headerTintLight;
            this.iconGap = iconGap;
            this.iconSize = iconSize;
            this.minBadgeWidth = minBadgeWidth;
            this.neutralBadgeAlpha = neutralBadgeAlpha;
            this.outerMargin = outerMargin;
            this.pingButtonWidth = pingButtonWidth;
            this.pingHoverAlpha = pingHoverAlpha;
            this.pingRestAlpha = pingRestAlpha;
            this.rowHeight = rowHeight;
            this.searchWidth = searchWidth;
            this.summaryHeight = summaryHeight;
            this.summaryPillAlpha = summaryPillAlpha;
            this.toolbarButtonWidth = toolbarButtonWidth;
        }
    }
}