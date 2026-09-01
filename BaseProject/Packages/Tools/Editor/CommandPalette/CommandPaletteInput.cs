using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Translates key presses into palette actions. Pure and free of any state, so the key map can
    /// be read in one place instead of being spread across the window.
    /// </summary>
    internal static class CommandPaletteInput
    {
        /// <summary>Reads the action the current event asks for.</summary>
        /// <param name="current">The event being processed.</param>
        /// <returns>The requested action, or <see cref="ECommandPaletteAction.None"/>.</returns>
        internal static ECommandPaletteAction Read(Event current)
        {
            if (current.type != EventType.KeyDown)
                return ECommandPaletteAction.None;

            return current.keyCode switch
            {
                KeyCode.Escape => ECommandPaletteAction.Close,
                KeyCode.DownArrow => ECommandPaletteAction.MoveDown,
                KeyCode.UpArrow => ECommandPaletteAction.MoveUp,
                KeyCode.PageDown => ECommandPaletteAction.PageDown,
                KeyCode.PageUp => ECommandPaletteAction.PageUp,
                KeyCode.Tab => ECommandPaletteAction.EditTags,
                KeyCode.D when IsActionKey(current) => ECommandPaletteAction.TogglePin,
                KeyCode.R when IsActionKey(current) => ECommandPaletteAction.Rescan,
                KeyCode.Return or KeyCode.KeypadEnter => Submit(current),
                _ => ECommandPaletteAction.None
            };
        }

        /// <summary>Whether the event cancels the tag editor.</summary>
        /// <param name="current">The event being processed.</param>
        /// <returns><c>true</c> for Escape.</returns>
        internal static bool IsCancel(Event current) => current.type == EventType.KeyDown
            && current.keyCode == KeyCode.Escape;

        /// <summary>Whether the event confirms the tag editor.</summary>
        /// <param name="current">The event being processed.</param>
        /// <returns><c>true</c> for either Enter key.</returns>
        internal static bool IsSubmit(Event current) => current.type == EventType.KeyDown
            && current.keyCode is KeyCode.Return or KeyCode.KeypadEnter;

        // EditorGUI.actionKey is not part of the public API, so the platform key is read here.
        private static bool IsActionKey(Event current) => current.control || current.command;

        private static ECommandPaletteAction Submit(Event current) => IsActionKey(current)
            ? ECommandPaletteAction.OpenScript
            : ECommandPaletteAction.Run;
    }
}