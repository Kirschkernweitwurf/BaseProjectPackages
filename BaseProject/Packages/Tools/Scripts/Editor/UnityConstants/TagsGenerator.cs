#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Base.ToolPackage.MenuManagerWindow;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditorInternal;

namespace Base.ToolPackage.Editor.UnityConstants
{
    /// <summary>
    /// Generates a class with all Unity Tags as const strings.
    /// </summary>
    public static class TagsGenerator
    {
        [DynamicMenuItem("Tools/Base Packages/Code Generation/Generate Tags")]
        public static void Generate()
        {
            GeneratorUtility.EnsureFolderExists(GeneratorUtility.OutputFolder);

            string[] tags = InternalEditorUtility.tags;

            StringBuilder sb = new();
            GeneratorUtility.WriteFileHeader(sb);
            sb.AppendLine($"namespace {GeneratorUtility.GeneratedNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>All Tags defined in the project, as const strings.</summary>");
            sb.AppendLine("    public static class Tags");
            sb.AppendLine("    {");

            HashSet<string> usedNames = new();
            foreach (string tag in tags)
            {
                string fieldName = GeneratorUtility.MakeUniqueIdentifier(tag, usedNames);
                sb.AppendLine($"        public const string {fieldName} = \"{GeneratorUtility.Escape(tag)}\";");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            GeneratorUtility.WriteFile("Tags.cs", sb.ToString());
            AssetDatabase.Refresh();
            CustomLogger.Log("Tags generated in " + GeneratorUtility.OutputFolder, null);
        }
    }
}
#endif