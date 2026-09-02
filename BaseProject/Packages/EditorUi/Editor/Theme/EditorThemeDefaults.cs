namespace Base.EditorUIPackage.Editor
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
        public static EditorThemeMetrics CreateMetrics() => new(16f,
            14f,
            6,
            11,
            8f,
            1f,
            20f,
            0.06f,
            14f,
            8f,
            8,
            18f,
            0.08f,
            22f,
            6f,
            12f,
            1f,
            8f,
            4f,
            15);

        /// <summary>
        /// The numbers a list window is built from.
        /// </summary>
        /// <returns>A fresh set, safe for the caller to keep.</returns>
        public static EditorThemeTable CreateTable() => new(0.30f,
            10,
            4,
            44f,
            6f,
            4f,
            0.07f,
            0.06f,
            3f,
            14f,
            78f,
            0.12f,
            6f,
            44f,
            0.45f,
            0.17f,
            24f,
            190f,
            24f,
            0.26f,
            62f);
    }
}