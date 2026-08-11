using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>Window to arrange dynamic menu item entries.</summary>
    internal sealed class MenuItemManagerWindow : MenuManagerWindowBase
    {
        private const string WindowTitle = "Menu Items";

        /// <inheritdoc/>
        protected override EMenuEntryKind Kind => EMenuEntryKind.MenuItem;

        /// <summary>Opens or focuses the window and returns it.</summary>
        public static MenuItemManagerWindow OpenWindow()
        {
            MenuItemManagerWindow window = GetWindow<MenuItemManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
            return window;
        }

        /// <summary>Opens the window and highlights the entry with the given id.</summary>
        public static void OpenAt(string entryId) => OpenWindow().FocusEntry(entryId);

        [MenuItem("Tools/Base Packages/Menu Management/Menu Item Manager", false, MenuPriority)]
        private static void Open() => OpenWindow();
    }
}