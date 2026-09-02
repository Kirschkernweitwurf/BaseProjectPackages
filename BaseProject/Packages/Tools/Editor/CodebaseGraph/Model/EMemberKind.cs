namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Category a scanned member falls into.</summary>
    internal enum EMemberKind : byte
    {
        Field = 0,
        SerializedField = 1,
        Const = 2,
        Property = 3,
        Method = 4,
        Constructor = 5,
        Event = 6,
        EnumMember = 7
    }
}