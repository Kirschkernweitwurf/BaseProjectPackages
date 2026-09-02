namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>What kind of entry a dismissal id points at.</summary>
    internal enum EDismissalKind : byte
    {
        Namespace = 0,
        Type = 1,
        Member = 2
    }
}