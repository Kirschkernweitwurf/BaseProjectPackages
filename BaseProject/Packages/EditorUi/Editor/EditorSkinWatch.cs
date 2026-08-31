namespace Base.EditorUiPackage
{
    /// <summary>
    /// Tracks whether a cache of styles still matches the editor skin and the theme it was built for.
    /// </summary>
    /// <remarks>
    /// <see cref="EditorStyleSet"/> builds on this for a style cache that is an object. A static one
    /// cannot inherit, and a static one is what a window ends up with whenever its styles are reached
    /// from free functions rather than through an instance. This is the same bookkeeping in a form
    /// such a class can hold in a field.
    /// <para>
    /// Asking and answering are two calls rather than one, because rebuilding can fail. Reading an
    /// editor style while a dropdown owns the GUI throws, and a cache that marked itself fresh before
    /// that happened would stay half built for the rest of the session. Only call
    /// <see cref="MarkFresh"/> once the rebuild actually finished.
    /// </para>
    /// </remarks>
    public sealed class EditorSkinWatch
    {
        private bool _isFresh;
        private bool _wasProSkin;
        private int _themeRevision;

        /// <summary>
        /// True when the styles have to be built: for the first time, because the user switched
        /// between the dark and the light skin, or because the active theme changed since.
        /// </summary>
        public bool IsStale => !_isFresh
            || _wasProSkin != EditorThemeProvider.IsDarkSkin
            || _themeRevision != EditorThemeProvider.Revision;

        /// <summary>Records that the styles are now built for the skin and theme currently active.</summary>
        public void MarkFresh()
        {
            _isFresh = true;
            _wasProSkin = EditorThemeProvider.IsDarkSkin;
            _themeRevision = EditorThemeProvider.Revision;
        }

        /// <summary>Forces the next check to report stale, for a cache that was torn down by hand.</summary>
        public void Invalidate() => _isFresh = false;
    }
}