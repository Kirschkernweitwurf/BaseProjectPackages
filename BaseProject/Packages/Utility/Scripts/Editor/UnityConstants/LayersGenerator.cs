#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using Base.ToolPackage.Editor.Generated;
using UnityEditor;
using UnityEngine;

namespace Base.UtilityPackage.Editor.UnityConstants
{
    /// <summary>
    /// Generates a class with all Unity Layers as const int indices (0-31)
    /// and a nested Masks class with their bit-shifted layer mask values.
    /// </summary>
    public static class LayersGenerator
    {
        private const int LayerCount = 32;

        [MenuItem("Tools/Base Packages/Code Generation/Generate Layers", priority = MenuOrders.Code)]
        public static void Generate()
        {
            GeneratorUtility.EnsureFolderExists(GeneratorUtility.OutputFolder);

            List<(int Index, string FieldName)> layers = new();
            HashSet<string> usedNames = new();
            for (int i = 0; i < LayerCount; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (string.IsNullOrEmpty(name))
                    continue;

                string fieldName = GeneratorUtility.MakeUniqueIdentifier(name, usedNames);
                layers.Add((i, fieldName));
            }

            StringBuilder sb = new();
            GeneratorUtility.WriteFileHeader(sb);
            sb.AppendLine($"namespace {GeneratorUtility.GeneratedNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>All Layers defined in the project, as const int indices (0-31).</summary>");
            sb.AppendLine("    public static class Layers");
            sb.AppendLine("    {");

            foreach ((int index, string fieldName) in layers)
                sb.AppendLine($"        public const int {fieldName} = {index};");

            sb.AppendLine();
            sb.AppendLine(
                "        /// <summary>Bit-shifted layer mask values (1 << index) for use with LayerMask fields.</summary>");

            sb.AppendLine("        public static class Masks");
            sb.AppendLine("        {");

            foreach ((int index, string fieldName) in layers)
                sb.AppendLine($"            public const int {fieldName} = 1 << {index};");

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            GeneratorUtility.WriteFile("Layers.cs", sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Layers generated in " + GeneratorUtility.OutputFolder, null);
        }
    }
}
#endif