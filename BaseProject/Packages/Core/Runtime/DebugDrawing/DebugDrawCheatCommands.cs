using Base.CorePackage.DebugMenu.CheatConsole;

namespace Base.CorePackage.DebugDrawing
{
    /// <summary>
    /// Exposes <see cref="DebugDraw"/> to the cheat console, so drawing can be silenced or cleared
    /// from inside a running build without a rebuild.
    /// </summary>
    internal static class DebugDrawCheatCommands
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string ClearCommand = "debugdraw_clear";
        private const string ClearDescription = "Removes everything debug draw is currently showing.";
        private const string ClearedMessage = "Debug draw cleared.";
        private const string EnabledCommand = "debugdraw_enabled";
        private const string EnabledDescription = "Switches debug drawing on or off.";
        private const string EnabledFormat = "Debug draw is now {0}.";
        private const string EnabledUsage = "debugdraw_enabled <true|false>";
        private const string OffState = "off";
        private const string OnState = "on";

        [CheatCommand(ClearCommand, Description = ClearDescription)]
        private static string Clear()
        {
            DebugDraw.Clear();

            return ClearedMessage;
        }

        [CheatCommand(EnabledCommand, Description = EnabledDescription, Usage = EnabledUsage)]
        private static string SetEnabled(bool value)
        {
            DebugDraw.SetEnabled(value);

            string state = value
                ? OnState
                : OffState;

            return string.Format(EnabledFormat, state);
        }
#endif
    }
}