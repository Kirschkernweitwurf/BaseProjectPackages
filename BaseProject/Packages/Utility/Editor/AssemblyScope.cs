using System;
using UnityEditor.Compilation;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Answers where a compiled assembly or an asset path sits: in the project, in an installed
    /// package, in a Unity package, or nowhere with a file behind it.
    /// </summary>
    /// <remarks>
    /// The prefixes and name rules here are the ones every tool that groups code by origin ends up
    /// writing for itself. This class holds them once. It deliberately returns plain facts rather than
    /// a verdict, because tools disagree on how fine the buckets should be: some only need project
    /// against everything else, others separate Unity packages from third party ones.
    /// </remarks>
    public static class AssemblyScope
    {
        private const string PackagePrefix = "Packages/";

        /// <summary>Name of the assembly loose scripts without an assembly definition compile into.</summary>
        private const string PlayerAssemblyPrefix = "Assembly-CSharp";

        private const string ProjectPrefix = "Assets/";
        private const string UnityPackagePrefix = "Packages/com.unity.";

        private static readonly string[] UnityNamePrefixes =
        {
            "Unity.",
            "UnityEngine.",
            "UnityEditor."
        };

        /// <summary>
        /// The assembly definition file an assembly was compiled from.
        /// </summary>
        /// <param name="assemblyName">The simple assembly name.</param>
        /// <returns>
        /// The project relative asmdef path, or an empty string for a precompiled library and for the
        /// predefined assemblies, which have no definition file.
        /// </returns>
        public static string DefinitionPathOf(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return string.Empty;

            return CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName)
                ?? string.Empty;
        }

        /// <summary>
        /// Whether the assembly is one of the predefined ones Unity compiles loose scripts into.
        /// Those have no assembly definition file but are still project code.
        /// </summary>
        /// <param name="assemblyName">The simple assembly name.</param>
        /// <returns>True for Assembly-CSharp and its editor and first pass variants.</returns>
        public static bool IsPlayerAssembly(string assemblyName) => !string.IsNullOrEmpty(assemblyName)
            && assemblyName.StartsWith(PlayerAssemblyPrefix, StringComparison.Ordinal);

        /// <summary>
        /// Whether the assembly name marks it as Unity owned. Used when no definition path resolves,
        /// which is the case for the precompiled assemblies Unity ships.
        /// </summary>
        /// <param name="assemblyName">The simple assembly name.</param>
        /// <returns>True when the name carries a Unity prefix.</returns>
        public static bool HasUnityName(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return false;

            foreach (string prefix in UnityNamePrefixes)
            {
                if (assemblyName.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>Whether the path points inside the project's Assets folder.</summary>
        /// <param name="assetPath">A project relative asset path.</param>
        /// <returns>True for a path under Assets.</returns>
        public static bool IsProjectPath(string assetPath) => !string.IsNullOrEmpty(assetPath)
            && assetPath.StartsWith(ProjectPrefix, StringComparison.Ordinal);

        /// <summary>
        /// Whether the path points inside any installed package, Unity's own ones included.
        /// </summary>
        /// <remarks>
        /// A Unity package satisfies this too, so ask <see cref="IsUnityPackagePath"/> first when the
        /// two have to be told apart.
        /// </remarks>
        /// <param name="assetPath">A project relative asset path.</param>
        /// <returns>True for a path under Packages.</returns>
        public static bool IsPackagePath(string assetPath) => !string.IsNullOrEmpty(assetPath)
            && assetPath.StartsWith(PackagePrefix, StringComparison.Ordinal);

        /// <summary>Whether the path points inside a package published by Unity.</summary>
        /// <param name="assetPath">A project relative asset path.</param>
        /// <returns>True for a path under a com.unity package.</returns>
        public static bool IsUnityPackagePath(string assetPath) => !string.IsNullOrEmpty(assetPath)
            && assetPath.StartsWith(UnityPackagePrefix, StringComparison.Ordinal);
    }
}