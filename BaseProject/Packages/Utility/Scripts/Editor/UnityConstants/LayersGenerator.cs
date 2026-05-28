using System.Collections.Generic;
using System.Text;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.UnityConstants
{
    /// <summary>
    /// Generates a class with all Unity Layers as const int indices (0-31).
    /// </summary>
    public static class LayersGenerator
    {
        [MenuItem("Tools/Base Packages/Code Generation/Generate Layers", priority = 2)]
        public static void Generate()
        {
            GeneratorUtility.EnsureFolderExists(GeneratorUtility.OutputFolder);

            StringBuilder sb = new();
            GeneratorUtility.WriteFileHeader(sb);
            sb.AppendLine($"namespace {GeneratorUtility.GeneratedNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>All Layers defined in the project, as const int indices (0-31).</summary>");
            sb.AppendLine("    public static class Layers");
            sb.AppendLine("    {");

            HashSet<string> usedNames = new();
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(name))
                    continue;

                string fieldName = GeneratorUtility.MakeUniqueIdentifier(name, usedNames);
                sb.AppendLine($"        public const int {fieldName} = {i};");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            GeneratorUtility.WriteFile("Layers.cs", sb.ToString());
            AssetDatabase.Refresh();
            CustomLogger.Log("Layers generated in " + GeneratorUtility.OutputFolder, null);
        }
    }
}