using System;
using Base.ToolPackage.Editor.Shared;
using UnityEditor;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Finds the most relevant source line for a script: the execution-order attribute
    /// when present, otherwise the class declaration.
    /// </summary>
    /// <remarks>
    /// The scan itself lives in <see cref="AttributeSourceLocator"/>. This only names the attribute
    /// and the line to fall back to, which is the whole of what makes this window's jump differ from
    /// the other overview windows' jumps.
    /// </remarks>
    internal static class ScriptDefinitionLocator
    {
        private const string AttributeToken = "DefaultExecutionOrder";

        /// <summary>
        /// Returns a one-based line and a zero-based column. The column points just inside
        /// the attribute's parentheses when present, otherwise the start of the line.
        /// </summary>
        /// <param name="script">The script to scan.</param>
        /// <param name="type">The type declared in it, used for the fallback line.</param>
        /// <returns>The line and column to place the cursor at, or zeros when nothing was found.</returns>
        internal static (int Line, int Column) Find(MonoScript script, Type type)
            => AttributeSourceLocator.Find(script, AttributeToken,
                AttributeSourceLocator.ClassDeclaration(type?.Name));
    }
}