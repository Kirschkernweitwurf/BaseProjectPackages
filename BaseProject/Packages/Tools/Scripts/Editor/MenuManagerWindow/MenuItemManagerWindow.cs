#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Window to arrange dynamic menu item entries.</summary>
    public sealed class MenuItemManagerWindow : MenuManagerWindowBase
    {
        private const string WindowTitle = "Menu Items";

        /// <inheritdoc/>
        protected override EMenuEntryKind Kind => EMenuEntryKind.MenuItem;

        [MenuItem("Tools/Base Packages/Menu Management/Menu Item Manager", false, MenuPriority)]
        private static void Open()
        {
            MenuItemManagerWindow window = GetWindow<MenuItemManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }
    }
}
#endif