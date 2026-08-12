using UnityEditor;
using UnityEditor.Toolbars;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Adds the palette button to Unity's main toolbar. The toolbar calls this method whenever it
    /// builds or refreshes its elements, so nothing has to be attached or timed by hand. The button
    /// can be hidden and moved from the toolbar's own context menu.
    /// </summary>
    internal static class CommandPaletteToolbar
    {
        private const string ElementPath = "Base/Command Palette";
        private const string IconName = "d_UnityEditor.ConsoleWindow";
        private const string Label = "Palette";
        private const string Tooltip = "Open the command palette (Ctrl+Shift+K)";

        /// <summary>Builds the button that opens the palette.</summary>
        /// <returns>The element the main toolbar renders.</returns>
        [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        private static MainToolbarElement CreatePaletteButton()
        {
            MainToolbarContent content = new(Label, EditorGUIUtility.FindTexture(IconName), Tooltip);

            return new MainToolbarButton(content, CommandPaletteWindow.Open);
        }
    }
}