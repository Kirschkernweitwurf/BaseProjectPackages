using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Compilation;
using Assembly = UnityEditor.Compilation.Assembly;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>Scans the project and builds the assembly graph data, including the reference check.</summary>
    public static class AssemblyGraphModel
    {
        private const string PackagePathPrefix = "Packages/";
        private const string UnityPackagePathPrefix = "Packages/com.unity.";

        /// <summary>Assemblies Unity adds by default. They are never shown and never reported.</summary>
        private static readonly HashSet<string> IgnoredAssemblyNames = new()
        {
            "UnityEditor.UI",
            "UnityEngine.UI",
            "UnityEditor.TestRunner",
            "UnityEngine.TestRunner"
        };

        /// <summary>Stands in for the source pass when an assembly has nothing left to explain.</summary>
        private static readonly HashSet<string> NoNamespaces = new();

        /// <summary>Name prefixes that mark an assembly as Unity owned when no package path resolves.</summary>
        private static readonly string[] UnityNamePrefixes =
        {
            "Unity.",
            "UnityEngine.",
            "UnityEditor."
        };

        /// <summary>Builds a node for every compiled assembly in the project.</summary>
        public static List<AssemblyNodeInfo> Build()
        {
            List<Assembly> visible = CollectVisible();
            HashSet<string> scanned = CollectScannedNames(visible);

            ReferenceUsageResolver resolver = new(BuildMetadataLookup(),
                AssemblyAncestryScanner.Scan(scanned),
                AssemblyNamespaceScanner.Scan(scanned));

            List<AssemblyNodeInfo> nodes = new(visible.Count);

            foreach (Assembly assembly in visible)
                nodes.Add(BuildNode(assembly, resolver));

            return nodes;
        }

        private static List<Assembly> CollectVisible()
        {
            Assembly[] compiled = CompilationPipeline.GetAssemblies(AssembliesType.Editor);
            List<Assembly> visible = new(compiled.Length);

            foreach (Assembly assembly in compiled)
            {
                if (IsIgnored(assembly.name))
                    continue;

                visible.Add(assembly);
            }

            return visible;
        }

        /// <summary>
        /// Every assembly the checks may have to answer for: the shown ones and everything they
        /// declare. Scanning wider than that would read Unity and library types for nothing.
        /// </summary>
        private static HashSet<string> CollectScannedNames(List<Assembly> visible)
        {
            HashSet<string> scanned = new(StringComparer.Ordinal);

            foreach (Assembly assembly in visible)
            {
                scanned.Add(assembly.name);

                foreach (Assembly reference in assembly.assemblyReferences)
                    scanned.Add(reference.name);
            }

            return scanned;
        }

        private static AssemblyNodeInfo BuildNode(Assembly assembly, ReferenceUsageResolver resolver)
        {
            string asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
            AssemblyNodeInfo node = new(assembly.name, asmdefPath, ResolveKind(assembly.name, asmdefPath));

            List<string> declared = CollectDeclared(assembly);
            HashSet<string> credited = resolver.CollectCredited(assembly.name);

            // Reading the source is the expensive part, so it only happens for an assembly that
            // still has something the compiled metadata cannot account for.
            HashSet<string> namespaces = HasUncredited(declared, credited)
                ? SourceUsingReader.Read(assembly.sourceFiles)
                : NoNamespaces;

            foreach (string reference in declared)
            {
                EReferenceStatus status = resolver.Resolve(credited, reference, namespaces);
                node.References.Add(new AssemblyReferenceInfo(reference, status));
            }

            return node;
        }

        private static List<string> CollectDeclared(Assembly assembly)
        {
            List<string> declared = new(assembly.assemblyReferences.Length);

            foreach (Assembly reference in assembly.assemblyReferences)
            {
                if (IsIgnored(reference.name))
                    continue;

                declared.Add(reference.name);
            }

            return declared;
        }

        private static bool HasUncredited(List<string> declared, HashSet<string> credited)
        {
            if (credited == null)
                return false;

            foreach (string reference in declared)
            {
                if (!credited.Contains(reference))
                    return true;
            }

            return false;
        }

        private static bool IsIgnored(string assemblyName) => IgnoredAssemblyNames.Contains(assemblyName);

        private static EAssemblyKind ResolveKind(string assemblyName, string asmdefPath)
        {
            if (string.IsNullOrEmpty(asmdefPath))
                return HasUnityNamePrefix(assemblyName)
                    ? EAssemblyKind.UnityPackage
                    : EAssemblyKind.Library;

            if (asmdefPath.StartsWith(UnityPackagePathPrefix, StringComparison.Ordinal))
                return EAssemblyKind.UnityPackage;

            if (asmdefPath.StartsWith(PackagePathPrefix, StringComparison.Ordinal))
                return HasUnityNamePrefix(assemblyName)
                    ? EAssemblyKind.UnityPackage
                    : EAssemblyKind.Package;

            return EAssemblyKind.Project;
        }

        private static bool HasUnityNamePrefix(string assemblyName)
        {
            foreach (string prefix in UnityNamePrefixes)
            {
                if (assemblyName.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Maps each loaded assembly name to the assembly names its compiled metadata records.
        /// That is what the runtime has to load, which is a floor under what the compilation needed
        /// rather than the whole of it, so it is one of three sources and not the answer on its own.
        /// </summary>
        private static Dictionary<string, HashSet<string>> BuildMetadataLookup()
        {
            Dictionary<string, HashSet<string>> lookup = new(StringComparer.Ordinal);

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (lookup.ContainsKey(name))
                    continue;

                HashSet<string> referenced = new(StringComparer.Ordinal);

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