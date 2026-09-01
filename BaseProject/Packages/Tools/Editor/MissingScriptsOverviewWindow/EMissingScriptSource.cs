namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// Identifies where a missing script was found.
    /// </summary>
    internal enum EMissingScriptSource : byte
    {
        Scene = 0,
        Prefab = 1,
        ScriptableObject = 2
    }
}