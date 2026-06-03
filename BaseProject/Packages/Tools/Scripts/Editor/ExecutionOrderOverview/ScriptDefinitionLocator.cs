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

        /// <summary>Returns a one-based line number, or zero when no good match is found.</summary>
        public static int FindLine(MonoScript script, Type type)
        {
            if (script == null || type == null)
                return 0;

            string source = script.text;
            if (string.IsNullOrEmpty(source))
                return 0;

            string[] lines = source.Replace("\r\n", "\n").Split('\n');
            string className = StripGenericSuffix(type.Name);
            Regex classPattern = new(@"\bclass\s+" + Regex.Escape(className) + @"\b");

            int attributeLine = -1;
            int classLine = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (attributeLine < 0 && lines[i].Contains(AttributeToken))
                    attributeLine = i + 1;

                if (classLine < 0 && classPattern.IsMatch(lines[i]))
                    classLine = i + 1;

                if (attributeLine > 0 && classLine > 0)
                    break;
            }

            if (attributeLine > 0)
                return attributeLine;

            return classLine > 0 ? classLine : 0;
        }

        private static string StripGenericSuffix(string typeName)
        {
            int tick = typeName.IndexOf('`');
            return tick >= 0 ? typeName[..tick] : typeName;
        }
    }
}