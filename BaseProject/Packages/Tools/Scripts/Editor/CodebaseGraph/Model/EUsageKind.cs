namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>How one member or type makes use of another.</summary>
    public enum EUsageKind : byte
    {
        Call = 0,
        VirtualCall = 1,
        Construct = 2,
        FieldRead = 3,
        FieldWrite = 4,
        DelegateReference = 5,
        Override = 6,
        InterfaceImplementation = 7,
        AttributeUsage = 8
    }
}
