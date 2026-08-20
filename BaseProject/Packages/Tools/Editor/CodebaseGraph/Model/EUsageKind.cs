namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// How one member or type makes use of another. A string reference is the weakest of them: the
    /// evidence is a literal that happens to match a member name, which is how Invoke, SendMessage and
    /// StartCoroutine reach code that no instruction ever points at.
    /// </summary>
    internal enum EUsageKind : byte
    {
        Call = 0,
        VirtualCall = 1,
        Construct = 2,
        FieldRead = 3,
        FieldWrite = 4,
        DelegateReference = 5,
        Override = 6,
        InterfaceImplementation = 7,
        AttributeUsage = 8,
        StringReference = 9
    }
}