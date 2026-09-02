// The main toolbar extension API landed in Unity 6.3. On older editors the button is left out and
// the palette is reached through the shortcut or the Tools menu.
#if UNITY_6000_3_OR_NEWER
using UnityEditor;
using UnityEditor.Toolbars;

namespace Base.ToolsPackage.Editor.CommandPalette
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
        private const string Tooltip = "Open the command palette (Ctrl+Shift+K)";

        /// <summary>Builds the button that opens the palette.</summary>
        /// <returns>The element the main toolbar renders.</returns>
        [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Middle)]
        private static MainToolbarElement CreatePaletteButton()
        {
            // An empty text keeps the button icon only while still carrying the tooltip.
            MainToolbarContent content = new(string.Empty, EditorGUIUtility.FindTexture(IconName), Tooltip);

            return new MainToolbarButton(content, CommandPaletteWindow.Open);
        }
    }
}
#endif