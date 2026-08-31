using System;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The spacings, sizes and corner radii the Base editor windows lay out by, read through
    /// <see cref="EditorMetrics"/>.
    /// </summary>
    /// <remarks>
    /// The radii and the hit width are clamped where they are read rather than where they are
    /// written, because the inspector writes the field directly and a negative radius asks for a
    /// texture with a negative edge length.
    /// </remarks>
    [Serializable]
    public sealed class EditorThemeMetrics
    {
        [SerializeField, Min(0f)] private float badgeHeight;
        [SerializeField, Min(0f)] private float badgePadding;
        [SerializeField, Min(1)] private int descriptionFontSize;
        [SerializeField, Min(0)] private int cardCornerRadius;
        [SerializeField, Min(1f)] private float dividerHitWidth;
        [SerializeField, Min(1f)] private float dividerThickness;
        [SerializeField, Min(0f)] private float headerHeight;
        [SerializeField, Range(0f, 1f)] private float hoverLift;
        [SerializeField, Min(0f)] private float indent;
        [SerializeField, Min(0f)] private float itemGap;
        [SerializeField, Min(0)] private int pillCornerRadius;
        [SerializeField, Min(0f)] private float pillHeight;
        [SerializeField, Range(0f, 1f)] private float pressDrop;
        [SerializeField, Min(1f)] private float rowHeight;
        [SerializeField, Min(0f)] private float rowInset;
        [SerializeField, Min(0f)] private float sectionGap;
        [SerializeField, Min(1f)] private float separatorThickness;
        [SerializeField, Min(0f)] private float sortArrowWidth;
        [SerializeField, Min(0f)] private float tightGap;
        [SerializeField, Min(1)] private int titleFontSize;

        /// <summary>Height of a badge or a chip.</summary>
        public float BadgeHeight => badgeHeight;

        /// <summary>Horizontal padding added to the measured text of a badge.</summary>
        public float BadgePadding => badgePadding;

        /// <summary>Corner radius of a card, a block or a button.</summary>
        public int CardCornerRadius => Mathf.Max(0, cardCornerRadius);

        /// <summary>Font size of the sentence under a window title.</summary>
        public int DescriptionFontSize => Mathf.Max(1, descriptionFontSize);

        /// <summary>Grab width of a draggable column divider.</summary>
        public float DividerHitWidth => Mathf.Max(1f, dividerHitWidth);

        /// <summary>Drawn width of a column divider.</summary>
        public float DividerThickness => Mathf.Max(1f, dividerThickness);

        /// <summary>Height of a column header strip.</summary>
        public float HeaderHeight => headerHeight;

        /// <summary>How much a background brightens while hovered.</summary>
        public float HoverLift => hoverLift;

        /// <summary>Horizontal offset one nesting level adds to a row.</summary>
        public float Indent => indent;

        /// <summary>Gap between two controls that belong together.</summary>
        public float ItemGap => itemGap;

        /// <summary>Corner radius of a pill or a status badge.</summary>
        public int PillCornerRadius => Mathf.Max(0, pillCornerRadius);

        /// <summary>Height of a pill.</summary>
        public float PillHeight => pillHeight;

        /// <summary>How much a background darkens while pressed.</summary>
        public float PressDrop => pressDrop;

        /// <summary>Height of a list row.</summary>
        public float RowHeight => Mathf.Max(1f, rowHeight);

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public float RowInset => rowInset;

        /// <summary>Gap between two sections of a window.</summary>
        public float SectionGap => sectionGap;

        /// <summary>Thickness of a hairline separator.</summary>
        public float SeparatorThickness => Mathf.Max(1f, separatorThickness);

        /// <summary>Width of the triangle marking the column a list is sorted by.</summary>
        public float SortArrowWidth => sortArrowWidth;

        /// <summary>Gap between two closely related controls.</summary>
        public float TightGap => tightGap;

        /// <summary>Font size of the name a window carries at its top.</summary>
        public int TitleFontSize => Mathf.Max(1, titleFontSize);

        /// <summary>Creates an empty set. Required by the serializer and by the inspector.</summary>
        public EditorThemeMetrics()
        {
        }

        /// <summary>Creates a full set of layout metrics.</summary>
        /// <param name="badgeHeight">Height of a badge or a chip.</param>
        /// <param name="badgePadding">Horizontal padding added to the measured text of a badge.</param>
        /// <param name="descriptionFontSize">Font size of the sentence under a window title.</param>
        /// <param name="cardCornerRadius">Corner radius of a card, a block or a button.</param>
        /// <param name="dividerHitWidth">Grab width of a draggable column divider.</param>
        /// <param name="dividerThickness">Drawn width of a column divider.</param>
        /// <param name="headerHeight">Height of a column header strip.</param>
        /// <param name="hoverLift">How much a background brightens while hovered.</param>
        /// <param name="indent">Horizontal offset one nesting level adds to a row.</param>
        /// <param name="itemGap">Gap between two controls that belong together.</param>
        /// <param name="pillCornerRadius">Corner radius of a pill or a status badge.</param>
        /// <param name="pillHeight">Height of a pill.</param>
        /// <param name="pressDrop">How much a background darkens while pressed.</param>
        /// <param name="rowHeight">Height of a list row.</param>
        /// <param name="rowInset">Horizontal padding at both ends of a row.</param>
        /// <param name="sectionGap">Gap between two sections of a window.</param>
        /// <param name="separatorThickness">Thickness of a hairline separator.</param>
        /// <param name="sortArrowWidth">Width of the sort triangle.</param>
        /// <param name="tightGap">Gap between two closely related controls.</param>
        /// <param name="titleFontSize">Font size of the name a window carries at its top.</param>
        public EditorThemeMetrics(float badgeHeight, float badgePadding, int cardCornerRadius,
            int descriptionFontSize, float dividerHitWidth, float dividerThickness, float headerHeight,
            float hoverLift, float indent, float itemGap, int pillCornerRadius, float pillHeight,
            float pressDrop, float rowHeight, float rowInset, float sectionGap, float separatorThickness,
            float sortArrowWidth, float tightGap, int titleFontSize)
        {
            this.badgeHeight = badgeHeight;
            this.badgePadding = badgePadding;
            this.descriptionFontSize = descriptionFontSize;
            this.cardCornerRadius = cardCornerRadius;
            this.dividerHitWidth = dividerHitWidth;
            this.dividerThickness = dividerThickness;
            this.headerHeight = headerHeight;
            this.hoverLift = hoverLift;
            this.indent = indent;
            this.itemGap = itemGap;
            this.pillCornerRadius = pillCornerRadius;
            this.pillHeight = pillHeight;
            this.pressDrop = pressDrop;
            this.rowHeight = rowHeight;
            this.rowInset = rowInset;
            this.sectionGap = sectionGap;
            this.separatorThickness = separatorThickness;
            this.sortArrowWidth = sortArrowWidth;
            this.tightGap = tightGap;
            this.titleFontSize = titleFontSize;
        }
    }
}