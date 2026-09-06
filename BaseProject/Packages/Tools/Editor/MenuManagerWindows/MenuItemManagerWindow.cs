using Base.ToolsPackage.Editor.MenuManagerModel;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.MenuManagerWindows
{
    /// <summary>Window to arrange dynamic menu item entries.</summary>
    internal sealed class MenuItemManagerWindow : MenuManagerWindowBase
    {
        private const string WindowTitle = "Menu Items";

        /// <inheritdoc/>
        protected override EMenuEntryKind Kind => EMenuEntryKind.MenuItem;

        /// <summary>Opens or focuses the window and returns it.</summary>
        internal static MenuItemManagerWindow OpenWindow()
        {
            MenuItemManagerWindow window = GetWindow<MenuItemManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
            return window;
        }

        /// <summary>Opens the window and highlights the entry with the given id.</summary>
        internal static void OpenAt(string entryId) => OpenWindow().FocusEntry(entryId);

        // The window that fixes a broken menu registration cannot be registered by the thing it fixes.
        // The Menu Manager reaches Unity's dynamic menu API by reflection, and if that binding ever
        // fails, every DynamicMenuItem in the project disappears at once. An entry for this window
        // would disappear with them, so it stays on Unity's MenuItem.
        [MenuItem("Tools/Base Packages/Menu Management/Menu Item Manager", false, MenuPriority)]
        private static void Open() => OpenWindow();
    }
}