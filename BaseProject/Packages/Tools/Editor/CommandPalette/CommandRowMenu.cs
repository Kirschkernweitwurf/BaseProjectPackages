using System;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// The context menu of a result row. Every item reports back as an action, so the window keeps
    /// one implementation per action no matter whether it came from the keyboard or the mouse.
    /// </summary>
    internal static class CommandRowMenu
    {
        private const string CopyPathLabel = "Copy Menu Path";
        private const string EditTagsLabel = "Edit Tags";
        private const string OpenScriptLabel = "Open Script";
        private const string PinLabel = "Pin";
        private const string UnpinLabel = "Unpin";

        /// <summary>Opens the context menu at the mouse position.</summary>
        /// <param name="entry">The entry the menu acts on.</param>
        /// <param name="handle">Receives the action the user picked.</param>
        internal static void Show(CommandEntry entry, Action<ECommandPaletteAction> handle)
        {
            GenericMenu menu = new();

            menu.AddItem(new GUIContent(EditTagsLabel), false, func: () => handle(ECommandPaletteAction.EditTags));

            menu.AddItem(new GUIContent(CommandTagStore.instance.IsPinned(entry.Id)
                ? UnpinLabel
                : PinLabel), false, func: () => handle(ECommandPaletteAction.TogglePin));

            menu.AddSeparator(string.Empty);

            if (CommandScriptOpener.CanOpen(entry))
                menu.AddItem(new GUIContent(OpenScriptLabel), false,
                    func: () => handle(ECommandPaletteAction.OpenScript));
            else
                menu.AddDisabledItem(new GUIContent(OpenScriptLabel));

            menu.AddItem(new GUIContent(CopyPathLabel), false,
                func: () => EditorGUIUtility.systemCopyBuffer = entry.Path);

            menu.ShowAsContext();
        }
    }
}