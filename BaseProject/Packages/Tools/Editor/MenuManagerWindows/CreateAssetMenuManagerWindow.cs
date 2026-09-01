using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Window to arrange dynamic asset creation entries.</summary>
    internal sealed class CreateAssetMenuManagerWindow : MenuManagerWindowBase
    {
        private const string WindowTitle = "Create Assets";

        /// <inheritdoc/>
        protected override EMenuEntryKind Kind => EMenuEntryKind.CreateAsset;

        /// <inheritdoc/>
        protected override bool ShowFileName => true;

        /// <summary>Opens or focuses the window and returns it.</summary>
        internal static CreateAssetMenuManagerWindow OpenWindow()
        {
            CreateAssetMenuManagerWindow window = GetWindow<CreateAssetMenuManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
            return window;
        }

        /// <summary>Opens the window and highlights the entry with the given id.</summary>
        internal static void OpenAt(string entryId) => OpenWindow().FocusEntry(entryId);

        [MenuItem("Tools/Base Packages/Menu Management/Create Asset Manager", false, MenuPriority)]
        private static void Open() => OpenWindow();
    }
}