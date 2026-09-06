using System.Text.RegularExpressions;
using Base.ToolsPackage.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>
    /// Finds the source line of a specific <see cref="CreateAssetMenuAttribute"/> so the editor
    /// can jump straight to the order argument.
    /// </summary>
    /// <remarks>
    /// The scan itself lives in <see cref="AttributeSourceLocator"/>. This only names the attribute,
    /// the argument the cursor should land on and the line to fall back to.
    /// </remarks>
    internal static class CreateAssetDefinitionLocator
    {
        private const string AttributeToken = "CreateAssetMenu";

        // CreateAssetMenu(... order = 120 ...) -> capture the order integer.
        private static readonly Regex OrderPattern = new(@"order\s*=\s*(-?\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Returns a one-based line and a zero-based column. The column points at the order
        /// value when present, otherwise just inside the attribute's parentheses. Falls back
        /// to the type declaration when the attribute line cannot be matched.
        /// </summary>
        /// <param name="script">The script to scan.</param>
        /// <param name="typeName">Name of the type carrying the attribute, used for the fallback line.</param>
        /// <returns>The line and column to place the cursor at, or zeros when nothing was found.</returns>
        internal static (int Line, int Column) Find(MonoScript script, string typeName) => AttributeSourceLocator.Find(
            script, AttributeToken,
            AttributeSourceLocator.ClassDeclaration(typeName), OrderPattern);
    }
}