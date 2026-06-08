using System;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Finds the source line of a specific <see cref="MenuItem"/> attribute so the editor
    /// can jump straight to the priority argument.
    /// </summary>
    public static class MenuItemDefinitionLocator
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
        public static (int Line, int Column) Find(MonoScript script, string menuPath, string methodName)
        {
            if (script == null)
                return (0, 0);

            string source = script.text;
            if (string.IsNullOrEmpty(source))
                return (0, 0);

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            string methodNeedle = methodName + "(";
            int methodLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.IndexOf(AttributeToken, StringComparison.Ordinal) >= 0
                    && line.IndexOf(menuPath, StringComparison.Ordinal) >= 0)
                    return (i + 1, ColumnFor(line));

                if (methodLine < 0 && line.IndexOf(methodNeedle, StringComparison.Ordinal) >= 0)
                    methodLine = i + 1;
            }

            return methodLine > 0 ? (methodLine, 0) : (0, 0);
        }

        private static int ColumnFor(string line)
        {
            Match priority = PriorityPattern.Match(line);
            if (priority.Success)
                return priority.Groups[1].Index;

            int tokenIndex = line.IndexOf(AttributeToken, StringComparison.Ordinal);
            int parenIndex = line.IndexOf('(', tokenIndex + AttributeToken.Length);
            return parenIndex >= 0 ? parenIndex + 1 : 0;
        }
    }
}