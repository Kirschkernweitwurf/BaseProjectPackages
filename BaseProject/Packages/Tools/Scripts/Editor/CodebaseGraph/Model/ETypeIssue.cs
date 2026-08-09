using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>Findings the analyzer can report for a whole type.</summary>
    [Flags]
    public enum ETypeIssue : byte
    {
        None = 0,
        DeadType = 1,
        GodClass = 2,
        TypeCycle = 4,
        HighInstability = 8,
        UnusedPublicType = 16
    }
}
