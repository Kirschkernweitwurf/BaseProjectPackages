using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace Base.UtilityPackage.Editor.UnityConstants
{
    /// <summary>
    /// Shared helper code for the Tags and Layers generators.
    /// Holds the output path, the namespace, and the name-cleaning logic.
    /// </summary>
    internal static class GeneratorUtility
    {
        public const string OutputFolder = "Assets/Generated/UnityConstants";
        public const string GeneratedNamespace = "Generated.UnityConstants";

        public static void WriteFileHeader(StringBuilder sb)
        {
            sb.AppendLine("// -----------------------------------------------------------------------------");
            sb.AppendLine("// AUTO-GENERATED FILE. DO NOT EDIT BY HAND.");
            sb.AppendLine("// Created by Tools > Base Packages > Code Generation.");
            sb.AppendLine("// Your changes will be overwritten on the next generation.");
            sb.AppendLine("// -----------------------------------------------------------------------------");
            sb.AppendLine();
        }

        public static void WriteFile(string fileName, string content)
        {
            string path = Path.Combine(OutputFolder, fileName);
            File.WriteAllText(path, content.TrimEnd('\r', '\n'));
        }

        public static void EnsureFolderExists(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        /// <summary>
        /// Converts an arbitrary string to a valid C# identifier by replacing invalid characters with underscores,
        /// prefixing with an underscore if it starts with a digit, and prefixing with "@" if it matches a C# keyword.
        /// </summary>
        public static string ToValidIdentifier(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "_";

            StringBuilder sb = new();
            foreach (char c in raw)
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');

            string result = sb.ToString();

            // C# names cannot start with a digit.
            if (char.IsDigit(result[0]))
                result = "_" + result;

            // Avoid clashing with C# keywords (e.g. "default", "object").
            if (Keywords.Contains(result))
                result = "@" + result;

            return result;
        }

        /// <summary>
        /// Converts a raw string to a valid C# identifier and ensures it's
        /// unique within the given set by appending a numeric suffix if needed.
        /// </summary>
        public static string MakeUniqueIdentifier(string raw, HashSet<string> used)
        {
            string baseName = ToValidIdentifier(raw);
            string name = baseName;
            int counter = 1;
            while (!used.Add(name))
            {
                name = baseName + "_" + counter;
                counter++;
            }
            return name;
        }

        public static string Escape(string value) => value.Replace("\\", @"\\").Replace("\"", "\\\"");

        private static readonly HashSet<string> Keywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch",
            "char", "checked", "class", "const", "continue", "decimal", "default",
            "delegate", "do", "double", "else", "enum", "event", "explicit",
            "extern", "false", "finally", "fixed", "float", "for", "foreach",
            "goto", "if", "implicit", "in", "int", "interface", "internal", "is",
            "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof",
            "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe",
            "ushort", "using", "virtual", "void", "volatile", "while"
        };
    }
}