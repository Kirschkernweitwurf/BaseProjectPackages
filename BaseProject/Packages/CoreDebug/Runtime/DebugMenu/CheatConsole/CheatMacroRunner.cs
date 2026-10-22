using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Base.CoreDebugPackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Loads every <see cref="CheatMacro"/> from the <c>Resources/CheatMacros</c> folders and registers each one as
    /// a console command. Loaded from Resources so a new macro works without touching the console prefab.
    /// </summary>
    internal static class CheatMacroRunner
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string LineFormat = "{0}: {1}";
        private const string ResourcesFolder = "CheatMacros";
        private const string SelfCallFormat = "skipped, {0} is already running.";

        // Guards against a macro that calls itself, directly or through another macro.
        private static readonly HashSet<CheatMacro> Running = new();

        /// <summary>Registers every macro in the Resources folders on <paramref name="model"/>.</summary>
        internal static void Register(CheatConsoleModel model)
        {
            foreach (CheatMacro macro in Resources.LoadAll<CheatMacro>(ResourcesFolder))
                model.RegisterBuiltinCommand(macro.Command, macro.Description, action: () => Run(model, macro));
        }

        private static string Run(CheatConsoleModel model, CheatMacro macro)
        {
            if (!Running.Add(macro))
                return string.Format(SelfCallFormat, macro.Command);

            StringBuilder output = new();

            try
            {
                foreach (string line in macro.Lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    CheatConsoleResult result = model.Run(line);
                    output.AppendLine(string.Format(LineFormat, line.Trim(), result.Message));
                }
            }
            finally
            {
                Running.Remove(macro);
            }

            return output.ToString().TrimEnd();
        }
#endif
    }
}