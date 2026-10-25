using Base.CoreDebugPackage.DebugMenu.CheatConsole;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Base.CoreDebugPackage.Editor.CheatConsole
{
    /// <summary>
    /// Keeps the editor from acting on the text shortcuts of the <see cref="CheatConsoleView"/> while someone types
    /// in it during play mode. Unity binds Ctrl+U to Undo History, which would open over the game. A shortcut of an
    /// active context wins over a global one, so these take the keys, do nothing, and the console in the game still
    /// gets them through the Input System.
    /// </summary>
    [InitializeOnLoad]
    internal sealed class CheatConsoleShortcutContext : IShortcutContext
    {
        private const string DeleteLineId = "Base/Cheat Console/Delete Line";
        private const string DeleteWordId = "Base/Cheat Console/Delete Word";

        static CheatConsoleShortcutContext() => ShortcutManager.RegisterContext(new CheatConsoleShortcutContext());

        /// <summary>Whether the console input has focus in play mode, so the shortcuts below take the keys.</summary>
        public bool active => IsTyping();

        // The console's own input action deletes the text; the editor only has to stay out of it.
        [Shortcut(DeleteWordId, typeof(CheatConsoleShortcutContext), KeyCode.W, ShortcutModifiers.Control)]
        private static void DeleteWord() { }

        [Shortcut(DeleteLineId, typeof(CheatConsoleShortcutContext), KeyCode.U, ShortcutModifiers.Control)]
        private static void DeleteLine() { }

        private static bool IsTyping()
        {
            if (!EditorApplication.isPlaying)
                return false;

            CheatConsoleView view = Object.FindAnyObjectByType<CheatConsoleView>();

            return view != null
                && view.InputField.isFocused;
        }
    }
}