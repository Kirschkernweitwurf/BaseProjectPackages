namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How the graph arranges what it draws. The two answer different questions: one shows what
    /// depends on what, the other shows where something is, and neither can do both at once.
    /// </summary>
    public enum ELayoutMode : byte
    {
        Dependencies = 0,
        Grouped = 1
    }
}
