using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Runs the commands each person picked for themselves as soon as the console finds them, for example a "Jonny"
    /// <see cref="CheatMacro"/> whenever a scene with its cheats loads. The list lives in this machine's PlayerPrefs,
    /// outside version control, and is edited in the console with autorun.add, autorun.remove and autorun.list.
    /// </summary>
    internal sealed class CheatAutoRun
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string AddCommand = "autorun.add";
        private const string AddedFormat = "{0} now runs whenever the console finds it.";
        private const string AlreadyAddedFormat = "{0} already runs automatically.";
        private const string EmptyListMessage = "No commands run automatically. Add one with autorun.add <command>.";
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

        // The objects behind each entry's commands at its last run. A scene load brings new objects, so the entry
        // runs again for them, but not for a second load that leaves them in place.
        private readonly Dictionary<string, List<object>> _lastTargets = new(StringComparer.OrdinalIgnoreCase);

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

        /// <summary>Whether anyone set up a command on this machine, so the console has to look for it.</summary>
        internal bool HasEntries => _entries.Count > 0;

        /// <summary>Registers the commands that edit the list.</summary>
        internal void Register()
        {
            _model.RegisterBuiltinCommand(AddCommand, "Runs a command whenever the console finds it, on this "
                + "machine only.", AddCommand + " <command>", Add);

            _model.RegisterBuiltinCommand(RemoveCommand, "Stops running a command automatically.",
                RemoveCommand + " <command>", Remove);

            _model.RegisterBuiltinCommand(ListCommand, "Lists the commands that run automatically on this machine.",
                ListEntries);
        }

        /// <summary>
        /// Runs every entry whose commands the console knows now and whose objects changed since its last run.
        /// </summary>
        internal void RunNewlyFound()
        {
            foreach (string entry in _entries)
            {
                List<object> targets = new();
                if (!TryCollectTargets(entry, targets, new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
                    continue;

                if (_lastTargets.TryGetValue(entry, out List<object> last)
                    && IsSameTargets(last, targets))
                    continue;

                _lastTargets[entry] = targets;
                CheatConsoleResult result = _model.Run(entry);
                CustomLogger.Log(string.Format(RanFormat, entry, result.Message), null);
            }
        }

        private static bool IsSameTargets(List<object> first, List<object> second)
        {
            if (first.Count != second.Count)
                return false;

            for (int i = 0; i < first.Count; i++)
            {
                if (!ReferenceEquals(first[i], second[i]))
                    return false;
            }

            return true;
        }

        private static string GetCommandName(string line)
        {
            string trimmed = line.Trim();
            int space = trimmed.IndexOf(WordSeparator);

            return space < 0
                ? trimmed
                : trimmed[..space];
        }

        private void Load()
        {
            string saved = PlayerPrefs.GetString(PrefsKey, string.Empty);

            foreach (string entry in saved.Split(EntrySeparator, StringSplitOptions.RemoveEmptyEntries))
                _entries.Add(entry.Trim());
        }

        private void Save() => PlayerPrefs.SetString(PrefsKey, string.Join(EntrySeparator, _entries));

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

            _entries.RemoveAt(index);
            _lastTargets.Remove(command);
            Save();

            return string.Format(RemovedFormat, command);
        }

        private string ListEntries() => HasEntries
            ? ListHeader + string.Join(ListSeparator, _entries)
            : EmptyListMessage;

        private int IndexOf(string command) =>
            _entries.FindIndex(entry => string.Equals(entry, command, StringComparison.OrdinalIgnoreCase));

        // A macro counts as found once every command its lines call is found, so "Jonny" waits for the date scene
        // instead of failing on the home screen.
        private bool TryCollectTargets(string command, List<object> targets, HashSet<string> visiting)
        {
            if (!_model.Commands.TryGetValue(command, out CheatCommandInfo info))
                return false;

            if (!_macros.TryGetValue(command, out CheatMacro macro))
            {
                targets.Add(info.Target);
                return true;
            }

            // The macro runner skips a macro that calls itself, so that line needs nothing.
            if (!visiting.Add(command))
                return true;

            foreach (string line in macro.Lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!TryCollectTargets(GetCommandName(line), targets, visiting))
                    return false;
            }

            return true;
        }
#endif
    }
}