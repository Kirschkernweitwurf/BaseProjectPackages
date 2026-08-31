namespace Base.EditorUiPackage
{
    /// <summary>
    /// The built-in look: the values the Base editor windows drew with before any of it could be
    /// themed, and the ones a project falls back to while no theme asset is assigned.
    /// </summary>
    /// <remarks>
    /// The colors are Slate, which is what the preset of that name applies too, so the look a project
    /// gets before it assigns anything is the same one it can pick deliberately later. Only the sizes
    /// are spelled out here.
    /// </remarks>
    public static class EditorThemeDefaults
    {
        /// <summary>
        /// The colors of the dark editor skin.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeColors CreateDarkColors()
            => EditorThemePresets.CreateColors(EEditorThemePreset.Slate, true);

        /// <summary>
        /// The colors of the light editor skin.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeColors CreateLightColors()
            => EditorThemePresets.CreateColors(EEditorThemePreset.Slate, false);

        /// <summary>
        /// The spacings, sizes and corner radii every window lays out by.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeMetrics CreateMetrics() => new(
            badgeHeight: 16f,
            badgePadding: 14f,
            cardCornerRadius: 6,
            descriptionFontSize: 11,
            dividerHitWidth: 8f,
            dividerThickness: 1f,
            headerHeight: 20f,
            hoverLift: 0.06f,
            indent: 14f,
            itemGap: 8f,
            pillCornerRadius: 8,
            pillHeight: 18f,
            pressDrop: 0.08f,
            rowHeight: 22f,
            rowInset: 6f,
            sectionGap: 12f,
            separatorThickness: 1f,
            sortArrowWidth: 8f,
            tightGap: 4f,
            titleFontSize: 15);

        /// <summary>
        /// The numbers a list window is built from.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeTable CreateTable() => new(
            badgeAlpha: 0.30f,
            badgeFontSize: 10,
            cardPadding: 4,
            emptyIconSize: 44f,
            emptyLineGap: 6f,
            headerArrowGap: 4f,
            headerTintDark: 0.07f,
            headerTintLight: 0.06f,
            iconGap: 3f,
            iconSize: 14f,
            minBadgeWidth: 78f,
            neutralBadgeAlpha: 0.12f,
            outerMargin: 6f,
            pingButtonWidth: 44f,
            pingHoverAlpha: 0.45f,
            pingRestAlpha: 0.17f,
            rowHeight: 24f,
            searchWidth: 190f,
            summaryHeight: 24f,
            summaryPillAlpha: 0.26f,
            toolbarButtonWidth: 62f);
    }
}