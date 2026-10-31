using System;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Keeps the commands typed into the console across play sessions and editor restarts, so the arrow keys still
    /// reach them next time. The history lives in this machine's PlayerPrefs, outside version control, so every
    /// person keeps their own. history.clear forgets it.
    /// </summary>
    internal sealed class CheatHistoryStore
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string ClearCommand = "history.clear";
        private const string ClearedMessage = "Command history cleared.";
        private const char EntrySeparator = '\n';
        private const string PrefsKey = "Base.CoreDebug.CheatHistory";

        private readonly CheatConsoleModel _model;

        /// <summary>Loads this machine's history into <paramref name="model"/>.</summary>
        /// <param name="model">The console model whose history is kept.</param>
        internal CheatHistoryStore(CheatConsoleModel model)
        {
            _model = model;

            string saved = PlayerPrefs.GetString(PrefsKey, string.Empty);
            _model.LoadHistory(saved.Split(EntrySeparator, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>Registers the command that forgets the history.</summary>
        internal void Register() =>
            _model.RegisterBuiltinCommand(ClearCommand, "Forgets the commands typed on this machine.", Clear);

        /// <summary>Saves the model's current history, after a command was typed.</summary>
        internal void Save() => PlayerPrefs.SetString(PrefsKey, string.Join(EntrySeparator, _model.History));

        private string Clear()
        {
            _model.ClearHistory();
            Save();

            return ClearedMessage;
        }
#endif
    }
}