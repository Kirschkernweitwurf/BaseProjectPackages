using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The built-in look: the values the Base editor windows drew with before any of it could be
    /// themed, and the ones a project falls back to while no theme asset is assigned.
    /// </summary>
    /// <remarks>
    /// This is the one file in the package that is allowed to spell numbers out. Everywhere else a
    /// size or a color is read from the active theme, and the theme is seeded from here.
    /// <para>
    /// A neutral overlay used to be written as an alpha that resolved to white on the dark skin and
    /// to black on the light one. Each skin now carries the finished color, which is why a few
    /// entries look like plain white or black at a low opacity.
    /// </para>
    /// </remarks>
    public static class EditorThemeDefaults
    {
        /// <summary>
        /// The colors of the dark editor skin.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeColors CreateDarkColors() => new(
            accent: new Color(0.32f, 0.60f, 0.94f),
            accentText: Color.white,
            background: new Color(0.17f, 0.17f, 0.19f),
            border: new Color(1f, 1f, 1f, 0.09f),
            card: new Color(0.22f, 0.22f, 0.24f),
            danger: new Color(0.86f, 0.34f, 0.36f),
            dimText: new Color(0.56f, 0.56f, 0.61f),
            divider: new Color(0f, 0f, 0f, 0.35f),
            field: new Color(0.13f, 0.13f, 0.15f),
            focus: new Color(0.95f, 0.75f, 0.25f),
            hover: new Color(1f, 1f, 1f, 0.05f),
            keyCap: new Color(1f, 1f, 1f, 0.10f),
            secondary: new Color(0.30f, 0.30f, 0.33f),
            secondaryText: new Color(0.86f, 0.86f, 0.88f),
            selection: new Color(0.32f, 0.60f, 0.94f, 0.90f),
            selectionFill: new Color(0.32f, 0.60f, 0.94f, 0.20f),
            separator: new Color(1f, 1f, 1f, 0.07f),
            stripe: new Color(1f, 1f, 1f, 0.03f),
            success: new Color(0.42f, 0.80f, 0.50f),
            text: new Color(0.88f, 0.88f, 0.90f),
            warning: new Color(0.95f, 0.65f, 0.25f));

        /// <summary>
        /// The colors of the light editor skin.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeColors CreateLightColors() => new(
            accent: new Color(0.20f, 0.48f, 0.86f),
            accentText: Color.white,
            background: new Color(0.83f, 0.83f, 0.85f),
            border: new Color(0f, 0f, 0f, 0.16f),
            card: new Color(0.85f, 0.85f, 0.87f),
            danger: new Color(0.80f, 0.26f, 0.28f),
            dimText: new Color(0.42f, 0.42f, 0.47f),
            divider: new Color(0f, 0f, 0f, 0.16f),
            field: new Color(0.95f, 0.95f, 0.96f),
            focus: new Color(0.95f, 0.75f, 0.25f),
            hover: new Color(0f, 0f, 0f, 0.05f),
            keyCap: new Color(0f, 0f, 0f, 0.08f),
            secondary: new Color(0.89f, 0.89f, 0.91f),
            secondaryText: new Color(0.18f, 0.18f, 0.20f),
            selection: new Color(0.20f, 0.48f, 0.86f, 0.90f),
            selectionFill: new Color(0.20f, 0.48f, 0.86f, 0.16f),
            separator: new Color(0f, 0f, 0f, 0.10f),
            stripe: new Color(0f, 0f, 0f, 0.03f),
            success: new Color(0.20f, 0.58f, 0.30f),
            text: new Color(0.13f, 0.13f, 0.15f),
            warning: new Color(0.85f, 0.52f, 0.12f));

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