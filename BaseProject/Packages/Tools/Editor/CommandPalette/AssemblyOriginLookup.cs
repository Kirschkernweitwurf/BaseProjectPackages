using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.Shared;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Classifies where a type's code lives by looking at its assembly instead of its script
    /// asset. The palette indexes thousands of commands, and one asset database search per
    /// command would make the first open take seconds.
    /// </summary>
    internal static class AssemblyOriginLookup
    {
        private const string PlayerAssemblyPrefix = "Assembly-CSharp";

        private static readonly Dictionary<Assembly, EAssetOrigin> Cache = new();

        /// <summary>Drops every cached classification.</summary>
        internal static void Clear() => Cache.Clear();

        /// <summary>Returns where the code of the given type comes from.</summary>
        /// <param name="type">The type to classify.</param>
        /// <returns>Project, package or built-in.</returns>
        internal static EAssetOrigin Classify(Type type)
        {
            if (type == null)
                return EAssetOrigin.BuiltIn;

            Assembly assembly = type.Assembly;

            if (Cache.TryGetValue(assembly, out EAssetOrigin cached))
                return cached;

            EAssetOrigin origin = Resolve(assembly);
            Cache[assembly] = origin;

            return origin;
        }

        private static EAssetOrigin Resolve(Assembly assembly)
        {
            string name = assembly.GetName().Name;

            // Loose scripts compile without an assembly definition file but are still project code.
            if (name.StartsWith(PlayerAssemblyPrefix, StringComparison.Ordinal))
                return EAssetOrigin.Project;

            string definition = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(name);

            return string.IsNullOrEmpty(definition)
                ? EAssetOrigin.BuiltIn
                : AssetOriginResolver.Classify(definition);
        }
    }
}