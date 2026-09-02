using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// What one pass over the project's scripts turned up: where each type is declared, and which files
    /// and type names came out of a generator.
    /// </summary>
    internal sealed class ScriptIndex
    {
        /// <summary>Asset path for each namespace qualified type name, resolved through MonoScript.</summary>
        internal Dictionary<string, string> ByFullName { get; }

        /// <summary>Asset path for each plain type name, read out of the source text.</summary>
        internal Dictionary<string, string> BySimpleName { get; }

        /// <summary>Asset paths whose header marks them as generated.</summary>
        internal HashSet<string> GeneratedPaths { get; }

        /// <summary>Plain names of every type declared inside a generated file.</summary>
        internal HashSet<string> GeneratedTypeNames { get; }

        /// <summary>
        /// Source of every script, kept only until the text pass has run. Reading the whole project
        /// twice was costing more than holding it once.
        /// </summary>
        internal Dictionary<string, string> Sources { get; }

        /// <summary>Creates an empty index.</summary>
        public ScriptIndex()
        {
            ByFullName = new Dictionary<string, string>(StringComparer.Ordinal);
            BySimpleName = new Dictionary<string, string>(StringComparer.Ordinal);
            GeneratedPaths = new HashSet<string>(StringComparer.Ordinal);
            GeneratedTypeNames = new HashSet<string>(StringComparer.Ordinal);
            Sources = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        /// <summary>Drops the held source once nothing needs it, so the scan does not keep it alive.</summary>
        internal void ReleaseSources() => Sources.Clear();

        /// <summary>True when the type at this path, or with this name, came out of a generator.</summary>
        /// <param name="scriptPath">Asset path of the script, or null.</param>
        /// <param name="simpleName">Plain name of the outermost type.</param>
        /// <returns>True when the type is generated.</returns>
        internal bool IsGenerated(string scriptPath, string simpleName)
        {
            if (!string.IsNullOrEmpty(scriptPath) && GeneratedPaths.Contains(scriptPath))
                return true;

            return !string.IsNullOrEmpty(simpleName) && GeneratedTypeNames.Contains(simpleName);
        }
    }
}