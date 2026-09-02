namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How the graph draws its relation lines. Muted is the default because a dense graph drawn at full
    /// strength is a wall of identical curves, and the point of a line is lost the moment every other
    /// line looks the same.
    /// </summary>
    internal enum EEdgeMode : byte
    {
        Muted = 0,

        // ReSharper disable once UnusedMember.Global
        All = 1,
        SelectedOnly = 2,
        None = 3
    }
}