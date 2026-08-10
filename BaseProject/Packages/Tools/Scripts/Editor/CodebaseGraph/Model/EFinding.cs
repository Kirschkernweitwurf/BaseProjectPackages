namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// A single finding the view can be narrowed down to. Member and type findings share one list,
    /// because from the toolbar they are all just "show me this one thing".
    /// </summary>
    internal enum EFinding : byte
    {
        None = 0,
        Any = 1,
        DeadMember = 2,
        SerializedNeverRead = 3,
        PublicButInternalOnly = 4,
        WriteOnlyField = 5,
        ReadOnlyCandidate = 6,
        StaticMutableState = 7,
        DeadType = 8,
        GodClass = 9,
        TypeCycle = 10,
        HighInstability = 11,
        NamespaceCycle = 12,
        PrivateCandidate = 13,
        UnusedPublicApi = 14,
        UnusedInterfaceMember = 15,
        UnimplementedInterfaceMember = 16
    }
}