using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AssemblyGraphPackage.Editor;
using UnityEditor.Compilation;
using Assembly = UnityEditor.Compilation.Assembly;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Scans the project and builds the assembly graph data, including unused reference detection.</summary>
    public static class AssemblyGraphModel
    {
        /// <summary>Assemblies Unity adds by default. They are never shown and never reported as unused.</summary>
        private static readonly HashSet<string> IgnoredAssemblyNames = new()
        {
            "UnityEditor.UI",
            "UnityEngine.UI",
            "UnityEditor.TestRunner",
            "UnityEngine.TestRunner"
        };

        /// <summary>Builds a node for every compiled assembly in the project.</summary>
        public static List<AssemblyNodeInfo> Build()
        {
            Assembly[] compiled = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            Dictionary<string, HashSet<string>> actualReferences = BuildActualReferenceLookup();

            List<AssemblyNodeInfo> nodes = new(compiled.Length);

            foreach (Assembly assembly in compiled)
            {
                if (IsIgnored(assembly.name))
                    continue;

                string asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
                AssemblyNodeInfo node = new(assembly.name, asmdefPath, ResolveKind(asmdefPath));

                actualReferences.TryGetValue(assembly.name, out HashSet<string> used);

                foreach (Assembly reference in assembly.assemblyReferences)
                {
                    if (IsIgnored(reference.name))
                        continue;

                    EReferenceStatus status = ResolveStatus(used, reference.name);
                    node.References.Add(new AssemblyReferenceInfo(reference.name, status));
                }

                nodes.Add(node);
            }

            return nodes;
        }

        private static bool IsIgnored(string assemblyName) => IgnoredAssemblyNames.Contains(assemblyName);

        private static EReferenceStatus ResolveStatus(HashSet<string> used, string referenceName)
        {
            if (used == null)
                return EReferenceStatus.Unknown;

            return used.Contains(referenceName)
                ? EReferenceStatus.Used
                : EReferenceStatus.Unused;
        }

        private static EAssemblyKind ResolveKind(string asmdefPath)
        {
            if (string.IsNullOrEmpty(asmdefPath))
                return EAssemblyKind.Library;

            if (asmdefPath.StartsWith("Packages/", StringComparison.Ordinal))
                return EAssemblyKind.Package;

            return EAssemblyKind.Project;
        }

        /// <summary>
        /// Maps each loaded assembly name to the set of assembly names it truly references,
        /// read from the compiled metadata in the current editor domain.
        /// </summary>
        private static Dictionary<string, HashSet<string>> BuildActualReferenceLookup()
        {
            Dictionary<string, HashSet<string>> lookup = new();

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (lookup.ContainsKey(name))
                    continue;

                HashSet<string> referenced = new();

                try
                {
                    foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                        referenced.Add(reference.Name);
                }
                catch
                {
                    // Dynamic or unreadable assemblies are skipped and stay Unknown.
                }

                lookup[name] = referenced;
            }

            return lookup;
        }
    }
}