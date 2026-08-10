using System.Collections.Generic;
using UnityEngine;

namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Contains built-in cheat commands such as help and clear.
    /// </summary>
    public static class BuiltinCheatCommands
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Bullet = "\u2022";
        private const string ClearCommand = "clear";
        private const string DescriptionColor = "#CFCFCF";
        private const string HelpCommand = "help";
        private const string Separator = "-----------";
        private const string TimescaleCommand = "log_timescale";
        private const string TimescaleFormat = "The current time scale is {0}.";
        private const string UsageColor = "#9FFCFD";

        /// <summary>
        /// Registers the built-in commands on the given model.
        /// </summary>
        /// <param name="model">The model the commands are registered on.</param>
        /// <param name="view">The view the commands write their output to.</param>
        public static void Register(CheatConsoleModel model, CheatConsoleView view)
        {
            model.RegisterBuiltinCommand(HelpCommand, "Lists all available cheat commands.",
                action: () => Help(model, view));

            model.RegisterBuiltinCommand(ClearCommand, "Clears the on-screen console log.",
                action: () => Clear(view));

            model.RegisterBuiltinCommand(TimescaleCommand, "Logs the current time scale.", LogTimescale);
        }

        private static string LogTimescale() => string.Format(TimescaleFormat, Time.timeScale);

        private static string Help(CheatConsoleModel model, CheatConsoleView view)
        {
            List<string> lines = new()
            {
                Separator,
                "Available commands:",
                Separator
            };

            foreach (KeyValuePair<string, CheatCommandInfo> pair in model.Commands)
            {
                string usage = string.IsNullOrWhiteSpace(pair.Value.Attribute.Usage)
                    ? pair.Key
                    : pair.Value.Attribute.Usage;

                string description = string.IsNullOrWhiteSpace(pair.Value.Attribute.Description)
                    ? string.Empty
                    : pair.Value.Attribute.Description;

                lines.Add($"{Bullet} <color={UsageColor}>{usage}</color>: "
                    + $"<color={DescriptionColor}>{description}</color>");
            }

            lines.Add(Separator);

            foreach (string line in lines)
                view.AppendLog(line, ECheatConsoleMessageType.Info);

            return "Displayed all available commands.";
        }

        private static string Clear(CheatConsoleView view)
        {
            view.ClearLog();
            return "Console log cleared.";
        }
#endif
    }
}