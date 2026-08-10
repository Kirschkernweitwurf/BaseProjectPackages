namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Column the list pane is currently sorted by.</summary>
    internal enum ESortMode : byte
    {
        Name = 0,
        FanIn = 1,
        FanOut = 2,
        Findings = 3
    }
}