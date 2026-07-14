#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Window to arrange dynamic asset creation entries.</summary>
    public sealed class CreateAssetMenuManagerWindow : MenuManagerWindowBase
    {
        private const string WindowTitle = "Create Assets";

        /// <inheritdoc/>
        protected override EMenuEntryKind Kind => EMenuEntryKind.CreateAsset;

        /// <inheritdoc/>
        protected override bool ShowFileName => true;

        [MenuItem("Tools/Base Packages/Menu Management/Create Asset Manager")]
        private static void Open()
        {
            CreateAssetMenuManagerWindow window = GetWindow<CreateAssetMenuManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }
    }
}
#endif