using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.PackageManager;
using AssembliesType = UnityEditor.Compilation.AssembliesType;
using CompilationAssembly = UnityEditor.Compilation.Assembly;
using CompilationPipeline = UnityEditor.Compilation.CompilationPipeline;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Decides which assemblies belong to the project. Everything under Assets counts, plus packages
    /// that are embedded, local or installed from Git, which is how the own packages arrive. Registry
    /// and built-in packages are never scanned, so the graph stays about the own code.
    /// <br/><br/>
    /// It also reports which of them are packages and which are test assemblies, because a public member
    /// of a distributable package is API rather than dead code. A test fixture is not production
    /// code even though it has to be scanned so that its calls keep production code alive.
    /// <br/><br/>
    /// The same scope answers a second question, which is which files a text pass may read. That is not
    /// a matter of taste: a registry or built-in package never references project code, so an identifier
    /// in one of them can never be a use of a project member. Reading them anyway lets an unrelated word
    /// silence a finding, and a silenced finding leaves no trace anywhere to notice it by.
    /// </summary>
    internal static class ProjectAssemblyResolver
    {
        private const string PredefinedAssemblyPrefix = "Assembly-CSharp";
        private const string ProjectPathPrefix = "Assets/";

        /// <summary>Assembly names whose presence marks a test assembly.</summary>
        private static readonly string[] TestAssemblyNames =
        {
            "nunit.framework",
            "UnityEngine.TestRunner",
            "UnityEditor.TestRunner"
        };

        /// <summary>Name prefixes that always mark an assembly as Unity owned.</summary>
        private static readonly string[] UnityNamePrefixes =
        {
            "Unity.",
            "UnityEngine.",
            "UnityEditor."
        };

        /// <summary>Returns every loaded assembly that holds project code.</summary>
        /// <param name="packageAssemblies">Receives the names of the ones that ship inside a package.</param>
        /// <param name="testAssemblies">Receives the names of the ones that hold tests.</param>
        /// <returns>The assemblies to scan, sorted by name.</returns>
        internal static List<Assembly> Resolve(HashSet<string> packageAssemblies, HashSet<string> testAssemblies)
        {
            HashSet<string> wanted = CollectWantedNames(packageAssemblies);
            List<Assembly> result = new();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                    continue;

                string name = assembly.GetName().Name;
                if (!wanted.Contains(name))
                    continue;

                if (IsTestAssembly(assembly))
                    testAssemblies.Add(name);

                result.Add(assembly);
            }

            result.Sort((left, right) => string.Compare(left.GetName().Name,
                right.GetName().Name,
                StringComparison.OrdinalIgnoreCase));

            return result;
        }

        /// <summary>
        /// Lists the source files of every assembly the scan covers, so a pass over the text reads the
        /// same code the pass over the compiled types does.
        /// </summary>
        /// <returns>Project relative paths, compared without case so Windows behaves.</returns>
        internal static HashSet<string> CollectProjectSourceFiles()
        {
            HashSet<string> files = new(StringComparer.OrdinalIgnoreCase);

            foreach (CompilationAssembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                if (HasUnityNamePrefix(assembly.name))
                    continue;

                string path = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);

                if (!IsWanted(assembly.name, path))
                    continue;

                foreach (string file in assembly.sourceFiles)
                    files.Add(file);
            }

            return files;
        }

        private static HashSet<string> CollectWantedNames(HashSet<string> packageAssemblies)
        {
            HashSet<string> wanted = new(StringComparer.Ordinal);

            foreach (CompilationAssembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                if (HasUnityNamePrefix(assembly.name))
                    continue;

                string asmdefPath =
                    CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);

                if (!IsWanted(assembly.name, asmdefPath))
                    continue;

                wanted.Add(assembly.name);

                if (!string.IsNullOrEmpty(asmdefPath) && IsOwnedPackage(asmdefPath))
                    packageAssemblies.Add(assembly.name);
            }

            return wanted;
        }

        private static bool IsTestAssembly(Assembly assembly)
        {
            foreach (AssemblyName referenced in assembly.GetReferencedAssemblies())
            {
                foreach (string marker in TestAssemblyNames)
                {
                    if (string.Equals(referenced.Name, marker, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        private static bool IsWanted(string assemblyName, string asmdefPath)
        {
            // Predefined assemblies have no asmdef but are always the user's own loose scripts.
            if (string.IsNullOrEmpty(asmdefPath))
                return assemblyName.StartsWith(PredefinedAssemblyPrefix, StringComparison.Ordinal);

            if (asmdefPath.StartsWith(ProjectPathPrefix, StringComparison.Ordinal))
                return true;

            return IsOwnedPackage(asmdefPath);
        }

        private static bool IsOwnedPackage(string asmdefPath)
        {
            PackageInfo package = PackageInfo.FindForAssetPath(asmdefPath);
            if (package == null)
                return false;

            return package.source == PackageSource.Embedded
                || package.source == PackageSource.Local
                || package.source == PackageSource.LocalTarball
                || package.source == PackageSource.Git;
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
    }
}