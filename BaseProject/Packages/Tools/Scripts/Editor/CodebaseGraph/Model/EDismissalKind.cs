namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>What kind of entry a dismissal id points at.</summary>
    public enum EDismissalKind : byte
    {
        Namespace = 0,
        Type = 1,
        Member = 2
    }
}
