namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Declared visibility of a type or member, ordered from most to least restricted.</summary>
    public enum EAccessLevel : byte
    {
        Private = 0,
        Protected = 1,
        Internal = 2,
        ProtectedInternal = 3,
        Public = 4
    }
}
