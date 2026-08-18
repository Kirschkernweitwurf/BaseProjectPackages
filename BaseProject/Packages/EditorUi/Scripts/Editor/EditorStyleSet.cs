using UnityEditor;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Base class for a window's style cache. Styles have to be built inside a GUI call because
    /// <see cref="EditorStyles"/> is not valid before that, and they have to be rebuilt when the
    /// user switches between the dark and light skin. Both are handled here, so a window only
    /// implements <see cref="Build"/>.
    /// </summary>
    /// <remarks>
    /// Call <see cref="EnsureBuilt"/> at the top of <c>OnGUI</c> and <see cref="Dispose"/> from
    /// <c>OnDisable</c>.
    /// </remarks>
    public abstract class EditorStyleSet
    {
        /// <summary>The textures generated for this style set, released on every rebuild.</summary>
        protected EditorTextureCache Textures { get; } = new();

        private bool _isBuilt;
        private bool _wasBuiltForProSkin;

        /// <summary>Builds the styles once, and again after a skin change. Call from <c>OnGUI</c>.</summary>
        public void EnsureBuilt()
        {
            if (_isBuilt && _wasBuiltForProSkin == EditorGUIUtility.isProSkin)
                return;

            Textures.Release();
            Build();

            _isBuilt = true;
            _wasBuiltForProSkin = EditorGUIUtility.isProSkin;
        }

        /// <summary>Releases the generated textures. Call from <c>OnDisable</c>.</summary>
        public void Dispose()
        {
            Textures.Release();
            _isBuilt = false;
        }

        /// <summary>Creates every style of this set. Runs inside a GUI call, so skin styles are valid.</summary>
        protected abstract void Build();
    }
}