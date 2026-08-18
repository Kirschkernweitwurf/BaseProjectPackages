using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Fonts the Base windows need but the editor skin does not provide.
    /// </summary>
    public static class EditorFonts
    {
        private static readonly string[] MonospacedCandidates =
        {
            "Consolas",
            "Menlo",
            "Monaco",
            "Courier New",
            "DejaVu Sans Mono"
        };

        private static Font _monospaced;

        /// <summary>
        /// A monospaced OS font for source and other text whose columns have to line up. Source read
        /// in a proportional font loses the alignment that makes it scannable.
        /// </summary>
        /// <remarks>
        /// Created once for the domain and deliberately never destroyed. Destroying it with the
        /// owning window left every <see cref="GUIStyle"/> built from it pointing at nothing, which
        /// Unity reports as a deleted invalid font reference on the next reload. The domain takes it
        /// with it either way.
        /// </remarks>
        /// <returns>The monospaced font, or null if the machine has none of the candidates, in which
        /// case a style keeps the editor default.</returns>
        public static Font Monospaced()
        {
            if (_monospaced != null)
                return _monospaced;

            _monospaced = Font.CreateDynamicFontFromOSFont(MonospacedCandidates, EditorStyles.textArea.fontSize);

            if (_monospaced != null)
                _monospaced.hideFlags = HideFlags.HideAndDontSave;

            return _monospaced;
        }
    }
}