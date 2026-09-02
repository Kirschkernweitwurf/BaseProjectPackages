using UnityEditor;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Base class for a window's style cache. Styles have to be built inside a GUI call because
    /// <see cref="EditorStyles"/> is not valid before that, they have to be rebuilt when the user
    /// switches between the dark and light editor theme, and again when the active theme changes. All three
    /// are handled here, so a window only implements <see cref="Build"/>.
    /// </summary>
    /// <remarks>
    /// Call <see cref="EnsureBuilt"/> at the top of <c>OnGUI</c> and <see cref="Dispose"/> from
    /// <c>OnDisable</c>.
    /// </remarks>
    public abstract class EditorStyleSet
    {
        /// <summary>The textures generated for this style set, released on every rebuild.</summary>
        protected EditorTextureCache Textures { get; } = new();

        private readonly EditorStyleWatch _watch = new();

        /// <summary>Builds the styles once, and again after either theme changes. Call from <c>OnGUI</c>.</summary>
        public void EnsureBuilt()
        {
            if (!_watch.IsStale)
                return;

            Textures.Release();
            Build();

            _watch.MarkFresh();
        }

        /// <summary>Releases the generated textures. Call from <c>OnDisable</c>.</summary>
        public void Dispose()
        {
            Textures.Release();

            _watch.Invalidate();
        }

        /// <summary>
        /// Creates every style of this set. Runs inside a GUI call, so the editor's own styles are
        /// valid to read from.
        /// </summary>
        protected abstract void Build();
    }
}