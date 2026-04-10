using System.Collections.Generic;

namespace Systems.CheatConsole.Cheats
{
    /// <summary>
    /// Contains built-in cheat commands such as help and clear.
    /// </summary>
    public static class BuiltinCheatCommands
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string TimescaleFormat = "The current time scale is {0}.";

        public static void Register(CheatConsoleModel model, CheatConsoleView view)
        {
            model.RegisterBuiltinCommand("help", "Lists all available cheat commands.", () => Help(model, view));
            model.RegisterBuiltinCommand("clear", "Clears the on-screen console log.", () => Clear(view));
            model.RegisterBuiltinCommand("log_timescale", "Logs the current time scale.", LogTimescale);
        }

        private static string LogTimescale()
        {
            float timeScale = UnityEngine.Time.timeScale;
            return string.Format(TimescaleFormat, timeScale);
        }

        private static string Help(CheatConsoleModel model, CheatConsoleView view)
        {
            List<string> lines = new()
            {
                "-----------",
                "Available commands:",
                "-----------"
            };

            foreach (KeyValuePair<string, CheatCommandInfo> pair in model.Commands)
            {
                string usage = string.IsNullOrWhiteSpace(pair.Value.Attribute.Usage)
                    ? pair.Key
                    : pair.Value.Attribute.Usage;

                string desc = string.IsNullOrWhiteSpace(pair.Value.Attribute.Description)
                    ? ""
                    : pair.Value.Attribute.Description;

                string descColored  = $"<color=#888888>{desc}</color>";
                lines.Add($"• {usage}: {descColored}");
            }

            lines.Add("-----------");

            foreach (string line in lines)
                view.AppendLog(line, CheatConsoleMessageType.Info);

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