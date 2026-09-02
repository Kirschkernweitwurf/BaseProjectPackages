using System;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Model
{
    /// <summary>
    /// Findings the analyzer can report for a single member. Backed by a ushort rather than a byte,
    /// because there are more than eight of them and a flags enum has to fit them all.
    /// </summary>
    [Flags]
    internal enum EMemberIssue : ushort
    {
        None = 0,
        DeadMember = 1,
        SerializedNeverRead = 2,
        PublicButInternalOnly = 4,
        WriteOnlyField = 8,
        ReadOnlyCandidate = 16,
        StaticMutableState = 32,
        PrivateCandidate = 64,
        UnusedPublicApi = 128,
        UnusedInterfaceMember = 256,
        UnimplementedInterfaceMember = 512
    }
}