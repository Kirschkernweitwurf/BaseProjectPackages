using UnityEditor;
using UnityEditorInternal;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The one place the active look is read from. <see cref="EditorPalette"/>,
    /// <see cref="EditorMetrics"/> and <see cref="EditorTableStyles"/> all forward to here, so
    /// assigning a theme changes every Base window at once and no window has to know a theme exists.
    /// </summary>
    /// <remarks>
    /// The resolved values are cached, because a list window reads several of them per row per
    /// repaint and resolving means a GUID lookup and an asset load. <see cref="NotifyChanged"/>
    /// drops the cache and raises <see cref="Revision"/>.
    /// <para>
    /// Interested caches poll <see cref="Revision"/> rather than subscribing to an event. A static
    /// event outlives play mode while domain reload is off, and would keep handing frames to windows
    /// that closed long ago; an integer cannot.
    /// </para>
    /// </remarks>
    public static class EditorThemeProvider
    {
        /// <summary>
        /// Rises by one every time the look changes. A style cache that recorded the value it built
        /// with knows it is stale as soon as the two differ.
        /// </summary>
        public static int Revision { get; private set; }

        /// <summary>The assigned theme, or null while the project draws with the built-in look.</summary>
        public static EditorTheme ActiveTheme
        {
            get
            {
                Resolve();

                return _theme;
            }
        }

        /// <summary>
        /// The editor theme the look currently resolves against: the one the editor runs, unless an override
        /// is in force.
        /// </summary>
        /// <remarks>
        /// Everything that used to branch on <see cref="EditorGUIUtility.isProSkin"/> asks this
        /// instead, which is what lets one panel be drawn in the other editor theme without the rest of the
        /// editor following it.
        /// </remarks>
        public static bool IsDarkMode => _darkModeOverride ?? EditorGUIUtility.isProSkin;

        /// <summary>The colors of the editor theme the look currently resolves against.</summary>
        public static EditorThemeColors Colors
        {
            get
            {
                Resolve();

                return IsDarkMode
                    ? _darkColors
                    : _lightColors;
            }
        }

        /// <summary>The spacings, sizes and corner radii every window lays out by.</summary>
        public static EditorThemeMetrics Metrics
        {
            get
            {
                Resolve();

                return _metrics;
            }
        }

        /// <summary>The numbers a list window is built from.</summary>
        public static EditorThemeTable Table
        {
            get
            {
                Resolve();

                return _table;
            }
        }

        // Built once for the domain and handed out by reference. Nothing here has a setter, so a
        // project on the built-in look allocates nothing at all on the path a window reads every
        // repaint.
        private static readonly EditorThemeColors DefaultDarkColors = EditorThemeDefaults.CreateDarkColors();
        private static readonly EditorThemeColors DefaultLightColors = EditorThemeDefaults.CreateLightColors();
        private static readonly EditorThemeMetrics DefaultMetrics = EditorThemeDefaults.CreateMetrics();
        private static readonly EditorThemeTable DefaultTable = EditorThemeDefaults.CreateTable();

        private static EditorTheme _theme;
        private static EditorThemeColors _darkColors = DefaultDarkColors;
        private static EditorThemeColors _lightColors = DefaultLightColors;
        private static EditorThemeMetrics _metrics = DefaultMetrics;
        private static EditorThemeTable _table = DefaultTable;

        private static bool? _darkModeOverride;
        private static bool _isResolved;
        private static bool _hasResolvedTheme;

        /// <summary>
        /// Points the project at a theme asset and refreshes every open window.
        /// </summary>
        /// <param name="theme">The theme to draw with, or null for the built-in look.</param>
        public static void SetActiveTheme(EditorTheme theme)
        {
            EditorThemeSettings.instance.SetThemeGuid(ResolveGuid(theme));

            NotifyChanged();
        }

        /// <summary>
        /// Resolves every color as though the editor were in the given mode until
        /// <see cref="EndDarkModeOverride"/> is called,
        /// preview can show the editor theme the editor is not running.
        /// </summary>
        /// <remarks>
        /// Only ever wrap the drawing of one panel in this, and always from a <c>finally</c>, because
        /// an override left standing would hand the wrong colors to every window that draws next.
        /// The styles of whatever is drawn inside have to be built inside it too: a style pins its
        /// text colors when it is built, not when it is drawn.
        /// </remarks>
        /// <param name="isDarkMode">True to resolve as dark mode, false as light mode.</param>
        public static void BeginDarkModeOverride(bool isDarkMode) => _darkModeOverride = isDarkMode;

        /// <summary>Hands the editor theme back to the one the editor is actually running.</summary>
        public static void EndDarkModeOverride() => _darkModeOverride = null;

        /// <summary>
        /// Drops the cached look, so the next read resolves it again, and repaints the editor.
        /// </summary>
        /// <remarks>
        /// Call after changing anything a window may already have built a style or a texture from.
        /// The theme asset does this for itself when it is edited.
        /// </remarks>
        public static void NotifyChanged()
        {
            _isResolved = false;
            Revision++;

            InternalEditorUtility.RepaintAllViews();
        }

        private static string ResolveGuid(EditorTheme theme)
        {
            if (theme == null)
                return string.Empty;

            string path = AssetDatabase.GetAssetPath(theme);

            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return AssetDatabase.AssetPathToGUID(path);
        }

        // A theme deleted or moved out from under the cache leaves a destroyed object behind, which
        // reads as null through Unity's operator but not as an empty field. Re-resolving on that is
        // what keeps a window from drawing with a look whose asset is gone. A theme that never
        // resolved in the first place is not retried here, or a stale GUID would make every read
        // walk the asset database again.
        private static bool IsCacheIntact() => !_hasResolvedTheme || _theme != null;

        private static void Resolve()
        {
            if (_isResolved && IsCacheIntact())
                return;

            string guid = EditorThemeSettings.instance.ThemeGuid;
            bool expectsTheme = !string.IsNullOrEmpty(guid);

            _theme = expectsTheme
                ? Load(guid)
                : null;

            _hasResolvedTheme = _theme != null;

            ApplyDefaults();

            if (_hasResolvedTheme)
                ApplyTheme();

            // An asset load early in a reload can come back empty while the database is still
            // catching up, so that one case is left open for the next read to try again. A GUID
            // that simply no longer exists settles on the built-in look instead of retrying
            // forever, and is picked up again the moment anything calls NotifyChanged.
            _isResolved = !expectsTheme || _hasResolvedTheme || !IsDatabaseBusy();
        }

        private static bool IsDatabaseBusy() => EditorApplication.isUpdating || EditorApplication.isCompiling;

        private static EditorTheme Load(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<EditorTheme>(path);
        }

        private static void ApplyDefaults()
        {
            _darkColors = DefaultDarkColors;
            _lightColors = DefaultLightColors;
            _metrics = DefaultMetrics;
            _table = DefaultTable;
        }

        // Block by block, so a theme missing one of them keeps the built-in values for that block
        // instead of falling back to the built-in look entirely.
        private static void ApplyTheme()
        {
            if (_theme.DarkColors != null)
                _darkColors = _theme.DarkColors;

            if (_theme.LightColors != null)
                _lightColors = _theme.LightColors;

            if (_theme.Metrics != null)
                _metrics = _theme.Metrics;

            if (_theme.Table != null)
                _table = _theme.Table;
        }
    }
}