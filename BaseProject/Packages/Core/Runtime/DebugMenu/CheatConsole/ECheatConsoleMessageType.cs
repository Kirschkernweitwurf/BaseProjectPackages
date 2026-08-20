namespace Base.CorePackage.DebugMenu.CheatConsole
{
    /// <summary>
    /// Represents the severity of a message written to the cheat console.
    /// </summary>
    public enum ECheatConsoleMessageType : byte
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Command = 3
    }
}