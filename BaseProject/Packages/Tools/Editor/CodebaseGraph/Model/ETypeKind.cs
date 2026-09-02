namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Category a scanned type falls into.</summary>
    internal enum ETypeKind : byte
    {
        Class = 0,
        Struct = 1,
        Interface = 2,
        Enum = 3,
        Delegate = 4
    }
}