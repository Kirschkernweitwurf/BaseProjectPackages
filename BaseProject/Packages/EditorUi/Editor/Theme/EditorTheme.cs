using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// A complete look for the Base editor windows: the colors for both editor themes, the spacings
    /// and corner radii every window lays out by, and the numbers a list window is built from.
    /// <para>
    /// Create one through the Editor UI Theme page in the project settings, assign it there, and
    /// every Base window picks it up. Nothing has to be edited in code.
    /// </para>
    /// </summary>
    /// <remarks>
    /// An asset rather than a settings file, so a look can be version controlled, shared between
    /// projects, and kept next to the project it belongs to. A project with no theme assigned draws
    /// with <see cref="EditorThemeDefaults"/>.
    /// </remarks>
    public sealed class EditorTheme : ScriptableObject
    {
        /// <summary>The serialized name of the dark editor colors, for the settings page to bind against.</summary>
        public const string DarkColorsPropertyName = nameof(darkColors);

        /// <summary>The serialized name of the light editor colors, for the settings page to bind against.</summary>
        public const string LightColorsPropertyName = nameof(lightColors);

        /// <summary>The serialized name of the layout metrics, for the settings page to bind against.</summary>
        public const string MetricsPropertyName = nameof(metrics);

        /// <summary>The serialized name of the list window metrics, for the settings page to bind against.</summary>
        public const string TablePropertyName = nameof(table);

        [SerializeField] private EditorThemeColors darkColors;
        [SerializeField] private EditorThemeColors lightColors;
        [SerializeField] private EditorThemeMetrics metrics;
        [SerializeField] private EditorThemeTable table;

        /// <summary>The colors used while the dark editor theme is active.</summary>
        public EditorThemeColors DarkColors => darkColors;

        /// <summary>The colors used while the light editor theme is active.</summary>
        public EditorThemeColors LightColors => lightColors;

        /// <summary>The spacings, sizes and corner radii every window lays out by.</summary>
        public EditorThemeMetrics Metrics => metrics;

        /// <summary>The numbers a list window is built from.</summary>
        public EditorThemeTable Table => table;

#region Unity Callbacks
        private void OnEnable() => FillMissing();

        // Editing a color in the inspector has to reach the windows that already built a style from
        // the old one, and a rebuild only happens once they are told the theme moved.
        private void OnValidate() => EditorThemeProvider.NotifyChanged();
#endregion

        /// <summary>
        /// Replaces the colors of both editor themes.
        /// </summary>
        /// <param name="dark">The colors for the dark editor theme.</param>
        /// <param name="light">The colors for the light editor theme.</param>
        public void SetColors(EditorThemeColors dark, EditorThemeColors light)
        {
            if (dark != null)
                darkColors = dark;

            if (light != null)
                lightColors = light;
        }

        /// <summary>
        /// Replaces the layout and list window metrics.
        /// </summary>
        /// <param name="layout">The spacings, sizes and corner radii.</param>
        /// <param name="list">The numbers a list window is built from.</param>
        public void SetMetrics(EditorThemeMetrics layout, EditorThemeTable list)
        {
            if (layout != null)
                metrics = layout;

            if (list != null)
                table = list;
        }

        /// <summary>
        /// Overwrites every value with the built-in look, discarding whatever was set.
        /// </summary>
        public void ResetToDefaults()
        {
            darkColors = EditorThemeDefaults.CreateDarkColors();
            lightColors = EditorThemeDefaults.CreateLightColors();
            metrics = EditorThemeDefaults.CreateMetrics();
            table = EditorThemeDefaults.CreateTable();
        }

        // A theme deserialized from an asset written by an older version can be missing a whole
        // block. Filling it here rather than at every read keeps the getters to a field access,
        // which matters because a window reads several of them per row per repaint.
        private void FillMissing()
        {
            darkColors ??= EditorThemeDefaults.CreateDarkColors();
            lightColors ??= EditorThemeDefaults.CreateLightColors();
            metrics ??= EditorThemeDefaults.CreateMetrics();
            table ??= EditorThemeDefaults.CreateTable();
        }
    }
}