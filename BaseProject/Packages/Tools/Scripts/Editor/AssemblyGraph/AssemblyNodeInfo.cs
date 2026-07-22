using System.Collections.Generic;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Category of an assembly, used for filtering and cleanup permission.</summary>
    public enum EAssemblyKind
    {
        Project,
        Package,
        UnityPackage,
        Library
    }

    /// <summary>Whether a declared reference appears to be used by the compiled assembly.</summary>
    public enum EReferenceStatus
    {
        Used,
        Unused,
        Unknown
    }

    /// <summary>A single declared reference from one assembly to another.</summary>
    public sealed class AssemblyReferenceInfo
    {
        public string TargetName { get; }

        public EReferenceStatus Status { get; }

        public bool IsUnused => Status == EReferenceStatus.Unused;

        public AssemblyReferenceInfo(string targetName, EReferenceStatus status)
        {
            TargetName = targetName;
            Status = status;
        }
    }

    /// <summary>One assembly node in the graph, with its declared references.</summary>
    public sealed class AssemblyNodeInfo
    {
        public string Name { get; }

        /// <summary>Asset path of the asmdef file. Null for predefined or precompiled assemblies.</summary>
        public string AsmdefPath { get; }

        public EAssemblyKind Kind { get; }

        public List<AssemblyReferenceInfo> References { get; }

        public bool HasAsmdef => !string.IsNullOrEmpty(AsmdefPath);

        /// <summary>First segment of the name, used to group assemblies by color.</summary>
        public string RootName
        {
            get
            {
                int dot = Name.IndexOf('.');
                return dot < 0
                    ? Name
                    : Name.Substring(0, dot);
            }
        }

        /// <summary>Only owned code may be edited. Unity packages and libraries are always off limits.</summary>
        public bool IsCleanable => HasAsmdef && (Kind == EAssemblyKind.Project || Kind == EAssemblyKind.Package);

        public bool HasUnusedReferences
        {
            get
            {
                foreach (AssemblyReferenceInfo reference in References)
                {
                    if (reference.IsUnused)
                        return true;
                }

                return false;
            }
        }

        public AssemblyNodeInfo(string name, string asmdefPath, EAssemblyKind kind)
        {
            Name = name;
            AsmdefPath = asmdefPath;
            Kind = kind;
            References = new List<AssemblyReferenceInfo>();
        }
    }
}