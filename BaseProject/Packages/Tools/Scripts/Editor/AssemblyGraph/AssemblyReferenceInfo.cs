namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>A single declared reference from one assembly to another.</summary>
    public sealed class AssemblyReferenceInfo
    {
        /// <summary>Name of the referenced assembly.</summary>
        public string TargetName { get; }

        /// <summary>Whether the reference is used, unused or undetermined.</summary>
        public EReferenceStatus Status { get; }

        /// <summary>True when the reference can safely be removed.</summary>
        public bool IsUnused => Status == EReferenceStatus.Unused;

        /// <summary>Creates a reference description.</summary>
        /// <param name="targetName">Name of the referenced assembly.</param>
        /// <param name="status">Result of the usage check.</param>
        public AssemblyReferenceInfo(string targetName, EReferenceStatus status)
        {
            TargetName = targetName;
            Status = status;
        }
    }
}