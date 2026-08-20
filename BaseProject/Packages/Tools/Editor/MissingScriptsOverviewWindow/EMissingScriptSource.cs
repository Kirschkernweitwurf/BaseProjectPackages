namespace Base.ToolPackage.Editor.MissingScriptsOverviewWindow
{
    /// <summary>
    /// Identifies where a missing script was found.
    /// </summary>
    public enum EMissingScriptSource : byte
    {
        Scene = 0,
        Prefab = 1,
        ScriptableObject = 2
    }
}