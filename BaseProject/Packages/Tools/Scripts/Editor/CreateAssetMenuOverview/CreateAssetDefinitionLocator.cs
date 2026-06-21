#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.CreateAssetMenuOverview
{
    /// <summary>
    /// Finds the source line of a specific <see cref="CreateAssetMenuAttribute"/> so the editor
    /// can jump straight to the order argument.
    /// </summary>
    public static class CreateAssetDefinitionLocator
    {
        private const string AttributeToken = "CreateAssetMenu";

        // CreateAssetMenu(... order = 120 ...) -> capture the order integer.
        private static readonly Regex OrderPattern =
            new(@"order\s*=\s*(-?\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Returns a one-based line and a zero-based column. The column points at the order
        /// value when present, otherwise just inside the attribute's parentheses. Falls back
        /// to the type declaration when the attribute line cannot be matched.
        /// </summary>
        public static (int Line, int Column) Find(MonoScript script, string typeName)
        {
            if (script == null)
                return (0, 0);

            string source = script.text;
            if (string.IsNullOrEmpty(source))
                return (0, 0);

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            string typeNeedle = "class " + typeName;
            int typeLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.IndexOf(AttributeToken, StringComparison.Ordinal) >= 0)
                    return (i + 1, ColumnFor(line));

                if (typeLine < 0 && line.IndexOf(typeNeedle, StringComparison.Ordinal) >= 0)
                    typeLine = i + 1;
            }

            return typeLine > 0
                ? (typeLine, 0)
                : (0, 0);
        }

        private static int ColumnFor(string line)
        {
            Match order = OrderPattern.Match(line);
            if (order.Success)
                return order.Groups[1].Index;

            int tokenIndex = line.IndexOf(AttributeToken, StringComparison.Ordinal);
            int parenIndex = line.IndexOf('(', tokenIndex + AttributeToken.Length);
            return parenIndex >= 0
                ? parenIndex + 1
                : 0;
        }
    }
}
#endif