namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>A single declared reference from one assembly to another.</summary>
    public sealed class AssemblyReferenceInfo
    {
        /// <summary>Name of the referenced assembly.</summary>
        public string TargetName { get; }

        /// <summary>
        /// True when nothing was found that needs the reference. That is a reason to look, not a
        /// verdict: the checks behind it can all miss, so removing one still has to be compiled.
        /// </summary>
        internal bool IsCandidate => Status == EReferenceStatus.Candidate;

        /// <summary>Whether the reference is used, a removal candidate or undetermined.</summary>
        private EReferenceStatus Status { get; }

        /// <summary>Creates a reference description.</summary>
        /// <param name="targetName">Name of the referenced assembly.</param>
        /// <param name="status">Result of the usage check.</param>
        internal AssemblyReferenceInfo(string targetName, EReferenceStatus status)
        {
            TargetName = targetName;
            Status = status;
        }
    }
}