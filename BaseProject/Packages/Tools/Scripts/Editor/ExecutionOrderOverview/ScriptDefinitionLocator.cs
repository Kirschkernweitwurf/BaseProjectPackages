#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Base.ToolPackage.Editor.ExecutionOrderOverview
{
    /// <summary>
    /// Finds the most relevant source line for a script: the execution-order attribute
    /// when present, otherwise the class declaration.
    /// </summary>
    public static class ScriptDefinitionLocator
    {
        private const string AttributeToken = "DefaultExecutionOrder";
        private const int ParenthesisOffset = 2;

        /// <summary>
        /// Returns a one-based line and a zero-based column. The column points just inside
        /// the attribute's parentheses when present, otherwise the start of the line.
        /// </summary>
        public static (int Line, int Column) Find(MonoScript script, Type type)
        {
            if (script == null || type == null)
                return (0, 0);

            string source = script.text;
            if (string.IsNullOrEmpty(source))
                return (0, 0);

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            string className = StripGenericSuffix(type.Name);
            Regex classPattern = new(@"\bclass\s+" + Regex.Escape(className) + @"\b");

            int classLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                int column = FindAttributeColumn(lines[i]);
                if (column >= 0)
                    return (i + 1, column);

                if (classLine < 0 && classPattern.IsMatch(lines[i]))
                    classLine = i + 1;
            }

            return classLine > 0
                ? (classLine, 0)
                : (0, 0);
        }

        private static int FindAttributeColumn(string line)
        {
            int tokenIndex = line.IndexOf(AttributeToken, StringComparison.Ordinal);
            if (tokenIndex < 0)
                return -1;

            int parenIndex = line.IndexOf('(', tokenIndex + AttributeToken.Length);
            if (parenIndex < 0)
                return tokenIndex;

            return parenIndex + ParenthesisOffset;
        }

        private static string StripGenericSuffix(string typeName)
        {
            int tick = typeName.IndexOf('`');
            return tick >= 0
                ? typeName[..tick]
                : typeName;
        }
    }
}
#endif