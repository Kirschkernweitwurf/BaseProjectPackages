namespace Base.EditorUiPackage
{
    /// <summary>
    /// The spacings and sizes the Base editor windows lay out by. A window keeps its own numbers for
    /// anything only it knows, such as the width of one particular button, and takes the rest from
    /// here so rows, gaps and hairlines line up across windows.
    /// </summary>
    public static class EditorMetrics
    {
        /// <summary>Height of a badge or a chip.</summary>
        public const float BadgeHeight = 16f;

        /// <summary>Horizontal padding added to the measured text of a badge.</summary>
        public const float BadgePadding = 14f;

        /// <summary>Corner radius of a card, a block or a button.</summary>
        public const int CardCornerRadius = 6;

        /// <summary>Grab width of a draggable column divider.</summary>
        public const float DividerHitWidth = 8f;

        /// <summary>Drawn width of a column divider.</summary>
        public const float DividerThickness = 1f;

        /// <summary>Height of a column header strip.</summary>
        public const float HeaderHeight = 20f;

        /// <summary>How much a background brightens while hovered.</summary>
        public const float HoverLift = 0.06f;

        /// <summary>Horizontal offset one nesting level adds to a row.</summary>
        public const float Indent = 14f;

        /// <summary>Gap between two controls that belong together.</summary>
        public const float ItemGap = 8f;

        /// <summary>Corner radius of a pill or a status badge.</summary>
        public const int PillCornerRadius = 8;

        /// <summary>Height of a pill.</summary>
        public const float PillHeight = 18f;

        /// <summary>How much a background darkens while pressed.</summary>
        public const float PressDrop = 0.08f;

        /// <summary>Height of a list row.</summary>
        public const float RowHeight = 22f;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public const float RowInset = 6f;

        /// <summary>Gap between two sections of a window.</summary>
        public const float SectionGap = 12f;

        /// <summary>Thickness of a hairline separator.</summary>
        public const float SeparatorThickness = 1f;

        /// <summary>Width of the triangle marking the column a list is sorted by.</summary>
        public const float SortArrowWidth = 8f;

        /// <summary>Gap between two closely related controls.</summary>
        public const float TightGap = 4f;
    }
}