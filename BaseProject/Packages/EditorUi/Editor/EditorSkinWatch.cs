using UnityEditor;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Tracks whether a cache of styles still matches the editor skin it was built for.
    /// </summary>
    /// <remarks>
    /// <see cref="EditorStyleSet"/> already answers this for a style cache that is an object. A
    /// static one cannot inherit, and a static one is what a window ends up with whenever its styles
    /// are reached from free functions rather than through an instance. This is the same two flags in
    /// a form such a class can hold in a field.
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

        /// <summary>
        /// True when the styles have to be built, either for the first time or because the user
        /// switched between the dark and the light skin since.
        /// </summary>
        public bool IsStale => !_isFresh || _wasProSkin != EditorGUIUtility.isProSkin;

        /// <summary>Records that the styles are now built for the skin that is currently active.</summary>
        public void MarkFresh()
        {
            _isFresh = true;
            _wasProSkin = EditorGUIUtility.isProSkin;
        }

        /// <summary>Forces the next check to report stale, for a cache that was torn down by hand.</summary>
        public void Invalidate() => _isFresh = false;
    }
}