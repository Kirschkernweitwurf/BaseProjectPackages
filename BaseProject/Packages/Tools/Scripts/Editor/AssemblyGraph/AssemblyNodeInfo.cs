using System.Collections.Generic;

namespace Base.AssemblyGraphPackage.Editor
{
    /// <summary>Category of an assembly, used for filtering and coloring.</summary>
    public enum EAssemblyKind
    {
        Project,
        Package,
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
        public AssemblyReferenceInfo(string targetName, EReferenceStatus status)
        {
            TargetName = targetName;
            Status = status;
        }

        public string TargetName { get; }
        public EReferenceStatus Status { get; }

        public bool IsUnused => Status == EReferenceStatus.Unused;
    }

    /// <summary>One assembly node in the graph, with its declared references.</summary>
    public sealed class AssemblyNodeInfo
    {
        public AssemblyNodeInfo(string name, string asmdefPath, EAssemblyKind kind)
        {
            Name = name;
            AsmdefPath = asmdefPath;
            Kind = kind;
            References = new List<AssemblyReferenceInfo>();
        }

        public string Name { get; }

        /// <summary>Asset path of the asmdef file. Null for predefined or precompiled assemblies.</summary>
        public string AsmdefPath { get; }

        public EAssemblyKind Kind { get; }
        public List<AssemblyReferenceInfo> References { get; }

        public bool HasAsmdef => !string.IsNullOrEmpty(AsmdefPath);

        public bool HasUnusedReferences
        {
            get
            {
                foreach (AssemblyReferenceInfo reference in References)
                {
                    if (reference.IsUnused) return true;
                }

                return false;
            }
        }
    }
}
