namespace Base.EditorUiPackage
{
    /// <summary>
    /// The spacings and sizes the Base editor windows lay out by. A window keeps its own numbers for
    /// anything only it knows, such as the width of one particular button, and takes the rest from
    /// here so rows, gaps and hairlines line up across windows.
    /// <para>
    /// Every value comes from the theme assigned in the Editor UI Theme project settings page, so a
    /// user can retune the layout without touching code.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Properties rather than constants on purpose. A constant is copied into every assembly that
    /// reads it at compile time, which would leave a themed value baked into whatever it happened to
    /// be when that assembly was last built.
    /// </remarks>
    public static class EditorMetrics
    {
        /// <summary>Height of a badge or a chip.</summary>
        public static float BadgeHeight => EditorThemeProvider.Metrics.BadgeHeight;

        /// <summary>Horizontal padding added to the measured text of a badge.</summary>
        public static float BadgePadding => EditorThemeProvider.Metrics.BadgePadding;

        /// <summary>Corner radius of a card, a block or a button.</summary>
        public static int CardCornerRadius => EditorThemeProvider.Metrics.CardCornerRadius;

        /// <summary>Font size of the sentence under a window title.</summary>
        public static int DescriptionFontSize => EditorThemeProvider.Metrics.DescriptionFontSize;

        /// <summary>Grab width of a draggable column divider.</summary>
        public static float DividerHitWidth => EditorThemeProvider.Metrics.DividerHitWidth;

        /// <summary>Drawn width of a column divider.</summary>
        public static float DividerThickness => EditorThemeProvider.Metrics.DividerThickness;

        /// <summary>Height of a column header strip.</summary>
        public static float HeaderHeight => EditorThemeProvider.Metrics.HeaderHeight;

        /// <summary>How much a background brightens while hovered.</summary>
        public static float HoverLift => EditorThemeProvider.Metrics.HoverLift;

        /// <summary>Horizontal offset one nesting level adds to a row.</summary>
        public static float Indent => EditorThemeProvider.Metrics.Indent;

        /// <summary>Gap between two controls that belong together.</summary>
        public static float ItemGap => EditorThemeProvider.Metrics.ItemGap;

        /// <summary>Corner radius of a pill or a status badge.</summary>
        public static int PillCornerRadius => EditorThemeProvider.Metrics.PillCornerRadius;

        /// <summary>Height of a pill.</summary>
        public static float PillHeight => EditorThemeProvider.Metrics.PillHeight;

        /// <summary>How much a background darkens while pressed.</summary>
        public static float PressDrop => EditorThemeProvider.Metrics.PressDrop;

        /// <summary>Height of a list row.</summary>
        public static float RowHeight => EditorThemeProvider.Metrics.RowHeight;

        /// <summary>Horizontal padding at both ends of a row.</summary>
        public static float RowInset => EditorThemeProvider.Metrics.RowInset;

        /// <summary>Gap between two sections of a window.</summary>
        public static float SectionGap => EditorThemeProvider.Metrics.SectionGap;

        /// <summary>Thickness of a hairline separator.</summary>
        public static float SeparatorThickness => EditorThemeProvider.Metrics.SeparatorThickness;

        /// <summary>Width of the triangle marking the column a list is sorted by.</summary>
        public static float SortArrowWidth => EditorThemeProvider.Metrics.SortArrowWidth;

        /// <summary>Gap between two closely related controls.</summary>
        public static float TightGap => EditorThemeProvider.Metrics.TightGap;

        /// <summary>Font size of the name a window carries at its top.</summary>
        public static int TitleFontSize => EditorThemeProvider.Metrics.TitleFontSize;
    }
}