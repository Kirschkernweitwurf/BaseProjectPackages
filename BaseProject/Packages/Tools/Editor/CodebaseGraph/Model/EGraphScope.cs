namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Zoom level the graph window currently renders.</summary>
    internal enum EGraphScope : byte
    {
        Namespace = 0,
        Type = 1,
        Member = 2
    }
}