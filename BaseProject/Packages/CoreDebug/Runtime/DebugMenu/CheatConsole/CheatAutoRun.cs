using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Runs the commands each person picked for themselves as soon as the console finds them, for example a "Jonny"
    /// <see cref="CheatMacro"/> once the scene with its cheats loads. Each runs once per play session, so a toggle
    /// stays on instead of flipping back on the next scene load. The list lives in this machine's PlayerPrefs,
    /// outside version control, and is edited in the console with autorun.add, autorun.remove and autorun.list.
    /// autorun.disable and autorun.enable switch an entry off and on again without losing it.
    /// </summary>
    internal sealed class CheatAutoRun
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string AddCommand = "autorun.add";
        private const string AddedFormat = "{0} now runs once per play session, as soon as the console finds it.";
        private const string AlreadyAddedFormat = "{0} already runs automatically.";
        private const string AlreadyDisabledFormat = "{0} is already disabled.";
        private const string AlreadyEnabledFormat = "{0} is already enabled.";
        private const string DisableCommand = "autorun.disable";
        private const string DisabledFormat = "{0} stays on the list but no longer runs automatically.";
        private const string DisabledPrefsKey = "Base.CoreDebug.CheatAutoRun.Disabled";
        private const string DisabledSuffix = " (disabled)";
        private const string EmptyListMessage = "No commands run automatically. Add one with autorun.add <command>.";
        private const string EnableCommand = "autorun.enable";
        private const string EnabledFormat = "{0} runs automatically again.";
        private const char EntrySeparator = '\n';
        private const string ListCommand = "autorun.list";
        private const string ListHeader = "Runs automatically: ";
        private const string ListSeparator = ", ";
        private const string NotAddedFormat = "{0} does not run automatically.";
        private const string PrefsKey = "Base.CoreDebug.CheatAutoRun";
        private const string RanFormat = "Auto-ran {0}: {1}";
        private const string RemoveCommand = "autorun.remove";
        private const string RemovedFormat = "{0} no longer runs automatically.";
        private const char WordSeparator = ' ';

        private readonly CheatConsoleModel _model;
        private readonly Dictionary<string, CheatMacro> _macros = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _entries = new();
        private readonly HashSet<string> _disabled = new(StringComparer.OrdinalIgnoreCase);

        // Kept when an entry is removed, so adding it again in the same session can't flip a toggle back.
        private readonly HashSet<string> _ran = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Creates the auto-run list of <paramref name="model"/> and loads this machine's entries.</summary>
        /// <param name="model">The console model the entries run on.</param>
        /// <param name="macros">The macros on the console, so an entry naming one waits for all of its lines.</param>
        internal CheatAutoRun(CheatConsoleModel model, IEnumerable<CheatMacro> macros)
        {
            _model = model;

            foreach (CheatMacro macro in macros)
                _macros[macro.Command] = macro;

            Load();
        }

        /// <summary>
        /// Whether an entry still waits for its commands this session, so the console has to keep looking for them.
        /// </summary>
        internal bool HasPending => _entries.Exists(IsWaiting);

        /// <summary>Registers the commands that edit the list.</summary>
        internal void Register()
        {
            _model.RegisterBuiltinCommand(AddCommand, "Runs a command once per play session as soon as the console "
                + "finds it, on this machine only.", AddCommand + " <command>", Add);

            _model.RegisterBuiltinCommand(RemoveCommand, "Stops running a command automatically.",
                RemoveCommand + " <command>", Remove);

            _model.RegisterBuiltinCommand(DisableCommand, "Keeps a command on the auto-run list but stops running it "
                + "until autorun.enable.", DisableCommand + " <command>", Disable);

            _model.RegisterBuiltinCommand(EnableCommand, "Runs a disabled auto-run command automatically again.",
                EnableCommand + " <command>", Enable);

            _model.RegisterBuiltinCommand(ListCommand, "Lists the commands that run automatically on this machine.",
                ListEntries);
        }

        /// <summary>
        /// Runs every enabled entry that has not run this session and whose commands the console knows now.
        /// </summary>
        internal void RunNewlyFound()
        {
            foreach (string entry in _entries)
            {
                if (!IsWaiting(entry)
                    || !IsFound(entry, new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
                    continue;

                _ran.Add(entry);
                CheatConsoleResult result = _model.Run(entry);
                CustomLogger.Log(string.Format(RanFormat, entry, result.Message), null);
            }
        }

        private static string GetCommandName(string line)
        {
            string trimmed = line.Trim();
            int space = trimmed.IndexOf(WordSeparator);

            return space < 0
                ? trimmed
                : trimmed[..space];
        }

        private static IEnumerable<string> LoadList(string prefsKey)
        {
            string saved = PlayerPrefs.GetString(prefsKey, string.Empty);

            foreach (string entry in saved.Split(EntrySeparator, StringSplitOptions.RemoveEmptyEntries))
                yield return entry.Trim();
        }

        private bool IsWaiting(string entry) => !_ran.Contains(entry) && !_disabled.Contains(entry);

        // Disabled entries are a separate key, so lists saved before disabling existed still load unchanged.
        private void Load()
        {
            _entries.AddRange(LoadList(PrefsKey));
            _disabled.UnionWith(LoadList(DisabledPrefsKey));
        }

        private void Save()
        {
            PlayerPrefs.SetString(PrefsKey, string.Join(EntrySeparator, _entries));
            PlayerPrefs.SetString(DisabledPrefsKey, string.Join(EntrySeparator, _disabled));
        }

        private string Add(string command)
        {
            if (IndexOf(command) >= 0)
                return string.Format(AlreadyAddedFormat, command);

            _entries.Add(command);
            Save();
            RunNewlyFound();

            return string.Format(AddedFormat, command);
        }

        private string Remove(string command)
        {
            int index = IndexOf(command);
            if (index < 0)
                return string.Format(NotAddedFormat, command);

            _disabled.Remove(_entries[index]);
            _entries.RemoveAt(index);
            Save();

            return string.Format(RemovedFormat, command);
        }

        private string Disable(string command)
        {
            int index = IndexOf(command);
            if (index < 0)
                return string.Format(NotAddedFormat, command);

            if (!_disabled.Add(_entries[index]))
                return string.Format(AlreadyDisabledFormat, command);

            Save();

            return string.Format(DisabledFormat, command);
        }

        private string Enable(string command)
        {
            int index = IndexOf(command);
            if (index < 0)
                return string.Format(NotAddedFormat, command);

            if (!_disabled.Remove(_entries[index]))
                return string.Format(AlreadyEnabledFormat, command);

            Save();
            RunNewlyFound();

            return string.Format(EnabledFormat, command);
        }

        private string ListEntries() => _entries.Count > 0
            ? ListHeader + string.Join(ListSeparator, _entries.ConvertAll(Describe))
            : EmptyListMessage;

        private string Describe(string entry) => _disabled.Contains(entry)
            ? entry + DisabledSuffix
            : entry;

        private int IndexOf(string command) =>
            _entries.FindIndex(entry => string.Equals(entry, command, StringComparison.OrdinalIgnoreCase));

        // A macro counts as found once every command its lines call is found, so "Jonny" waits for the date scene
        // instead of failing on the home screen.
        private bool IsFound(string command, HashSet<string> visiting)
        {
            if (!_model.Commands.ContainsKey(command))
                return false;

            // The macro runner skips a macro that calls itself, so that line needs nothing.
            if (!_macros.TryGetValue(command, out CheatMacro macro)
                || !visiting.Add(command))
                return true;

            foreach (string line in macro.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!IsFound(GetCommandName(line), visiting))
                    return false;
            }

            return true;
        }
#endif
    }
}