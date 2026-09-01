using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// The hint bar at the bottom of the palette: what the keys do on the left, a short status on
    /// the right.
    /// </summary>
    internal static class CommandPaletteFooter
    {
        private static readonly (string Key, string Label)[] Hints =
        {
            ("Enter", "Run"),
            ("Tab", "Add Tags"),
            ("Ctrl+D", "Pin"),
            ("Ctrl+Enter", "Open Script")
        };

        /// <summary>Draws the key hints and a status message.</summary>
        /// <param name="row">The footer row.</param>
        /// <param name="message">Right aligned status, usually the result count.</param>
        internal static void Draw(Rect row, string message)
        {
            float x = row.x;

            foreach ((string key, string label) in Hints)
                x = CommandPaletteChrome.DrawHint(row, x, key, label);

            GUI.Label(row, message, CommandPaletteStyles.CountLabel);
        }

        /// <summary>Draws a single line of text instead of the key hints.</summary>
        /// <param name="row">The footer row.</param>
        /// <param name="text">The text to show.</param>
        internal static void DrawText(Rect row, string text) => GUI.Label(row, text, CommandPaletteStyles.HintLabel);
    }
}