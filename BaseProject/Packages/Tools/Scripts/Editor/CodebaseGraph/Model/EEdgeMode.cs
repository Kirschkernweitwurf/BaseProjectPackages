namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How the graph draws its relation lines. Muted is the default because a dense graph drawn at full
    /// strength is a wall of identical curves, and the point of a line is lost the moment every other
    /// line looks the same.
    /// </summary>
    public enum EEdgeMode : byte
    {
        Muted = 0,
        All = 1,
        SelectedOnly = 2,
        None = 3
    }
}
