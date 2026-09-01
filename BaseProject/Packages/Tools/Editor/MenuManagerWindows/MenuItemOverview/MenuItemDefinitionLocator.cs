using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.Shared;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuManagerWindows.MenuItemOverview
{
    /// <summary>
    /// Finds the source line of a specific <see cref="MenuItem"/> attribute so the editor
    /// can jump straight to the priority argument.
    /// </summary>
    /// <remarks>
    /// The scan itself lives in <see cref="AttributeSourceLocator"/>. The menu path is passed as the
    /// text the line has to carry as well, which is what tells several menu items in one file apart.
    /// </remarks>
    internal static class MenuItemDefinitionLocator
    {
        private const string AttributeToken = "MenuItem";

        // MenuItem("path", true|false, priority) -> capture the priority integer.
        private static readonly Regex PriorityPattern =
            new(@"MenuItem\s*\(\s*""[^""]*""\s*,\s*(?:true|false)\s*,\s*(-?\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Returns a one-based line and a zero-based column. The column points at the
        /// priority value when present, otherwise just inside the attribute's parentheses.
        /// Falls back to the method declaration when the attribute line cannot be matched.
        /// </summary>
        /// <param name="script">The script to scan.</param>
        /// <param name="menuPath">The menu path, which tells this item from the others in the file.</param>
        /// <param name="methodName">Name of the method carrying the attribute, used for the fallback line.</param>
        /// <returns>The line and column to place the cursor at, or zeros when nothing was found.</returns>
        internal static (int Line, int Column) Find(MonoScript script, string menuPath, string methodName)
            => AttributeSourceLocator.Find(script, AttributeToken,
                AttributeSourceLocator.MemberDeclaration(methodName), PriorityPattern, menuPath);
    }
}