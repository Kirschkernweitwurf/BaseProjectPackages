using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>One assembly node in the graph, with its declared references.</summary>
    internal sealed class AssemblyNodeInfo
    {
        /// <summary>Name of the assembly.</summary>
        internal string Name { get; }

        /// <summary>Asset path of the asmdef file. Null for predefined or precompiled assemblies.</summary>
        internal string AsmdefPath { get; }

        /// <summary>Category the assembly falls into.</summary>
        internal EAssemblyKind Kind { get; }

        /// <summary>Every reference this assembly declares.</summary>
        internal List<AssemblyReferenceInfo> References { get; }

        /// <summary>True when the assembly is defined by an asmdef file.</summary>
        internal bool HasAsmdef => !string.IsNullOrEmpty(AsmdefPath);

        /// <summary>First segment of the name, used to group assemblies by color.</summary>
        internal string RootName
        {
            get
            {
                int dot = Name.IndexOf('.');
                return dot < 0
                    ? Name
                    : Name[..dot];
            }
        }

        /// <summary>Only owned code may be edited. Unity packages and libraries are always off limits.</summary>
        internal bool IsCleanable => HasAsmdef && (Kind == EAssemblyKind.Project || Kind == EAssemblyKind.Package);

        /// <summary>True when at least one declared reference is unused.</summary>
        internal bool HasUnusedReferences
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

        /// <summary>Creates an assembly node without any references yet.</summary>
        /// <param name="name">Name of the assembly.</param>
        /// <param name="asmdefPath">Asset path of the asmdef file, or null when there is none.</param>
        /// <param name="kind">Category the assembly falls into.</param>
        public AssemblyNodeInfo(string name, string asmdefPath, EAssemblyKind kind)
        {
            Name = name;
            AsmdefPath = asmdefPath;
            Kind = kind;
            References = new List<AssemblyReferenceInfo>();
        }
    }
}