using System;
using System.IO;
using System.Linq;
using System.Text;
using Base.CorePackage.MenuManaging.Identifier;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.CorePackage.Editor.MenuManaging.Identifier
{
    /// <summary>
    /// Editor utility that scans the project for all <see cref="MenuIdentifier"/> assets and generates
    /// a static accessor class for them, as well as a registry asset for runtime resolution.
    /// </summary>
    public static class MenuIdentifierGenerator
    {
        private const string DefaultRegistryDirectory = "Assets/Generated/Resources";
        private const string GeneratedClassName = "MenuIdentifiers";
        private const string GeneratedNamespace = "Base.CorePackage.MenuManaging.Generated";
        private const string OutputDirectory = "Assets/Generated/MenuIdentifiers";
        private const string RegenerateMenuPath = "Tools/Base Packages/Assets/Menu/Regenerate Menu Identifiers";
        private const string RegistryFileName = "MIR_Registry.asset";
        private const string ResourcesFolder = "/Resources/";

        private static string OutputPath => $"{OutputDirectory}/{GeneratedClassName}.cs";

        /// <summary>
        /// Rebuilds the registry asset and the generated accessor class from the identifiers in the project.
        /// </summary>
        [DynamicMenuItem(RegenerateMenuPath)]
        public static void Regenerate()
        {
            MenuIdentifier[] identifiers = LoadAllIdentifiers();

            if (HasDuplicateNames(identifiers))
                return;

            UpdateRegistry(identifiers);
            WriteAccessorClass(identifiers);
        }

        private static MenuIdentifier[] LoadAllIdentifiers() => AssetDatabase
            .FindAssets($"t:{nameof(MenuIdentifier)}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<MenuIdentifier>)
            .Where(identifier => identifier != null)
            .OrderBy(keySelector: identifier => identifier.name, StringComparer.Ordinal)
            .ToArray();

        /// <summary>
        /// Two assets that sanitize to the same name would generate invalid C#, so the run is aborted
        /// and every conflict is reported instead of silently dropping one of them.
        /// </summary>
        private static bool HasDuplicateNames(MenuIdentifier[] identifiers)
        {
            bool hasDuplicates = false;

            foreach (IGrouping<string, MenuIdentifier> group in identifiers.GroupBy(SanitizeAssetName))
            {
                if (group.Count() <= 1)
                    continue;

                hasDuplicates = true;
                string conflicts = string.Join(", ", group.Select(AssetDatabase.GetAssetPath));

                CustomLogger.LogError($"Duplicate {nameof(MenuIdentifier)} name \"{group.Key}\". "
                    + $"Conflicts: {conflicts}", null);
            }

            return hasDuplicates;
        }

        private static void UpdateRegistry(MenuIdentifier[] identifiers)
        {
            MenuIdentifierRegistry registry = ResolveRegistry();

            // Only write when the set actually changed, otherwise the asset gets rewritten on every
            // run and shows up as modified in version control for no reason.
            if (registry.EntriesEqual(identifiers))
                return;

            registry.SetEntries(identifiers);
            EditorUtility.SetDirty(registry);
        }

        /// <summary>
        /// Returns the single registry of the project. Finds it by type, so it can live in any
        /// directory the user picks. Creates one if none exists and deletes any duplicates.
        /// </summary>
        private static MenuIdentifierRegistry ResolveRegistry()
        {
            string[] paths = AssetDatabase.FindAssets($"t:{nameof(MenuIdentifierRegistry)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .OrderBy(keySelector: path => path, StringComparer.Ordinal)
                .ToArray();

            if (paths.Length == 0)
                return CreateRegistry();

            // Only one registry may exist. Keep the first by path for a stable, deterministic
            // choice, and drop the rest so no stale copy can be picked up at runtime.
            string keptPath = paths[0];

            for (int i = 1; i < paths.Length; i++)
            {
                CustomLogger.LogWarning($"Deleting duplicate {nameof(MenuIdentifierRegistry)} at \"{paths[i]}\". "
                    + $"Keeping \"{keptPath}\".", null);

                AssetDatabase.DeleteAsset(paths[i]);
            }

            WarnIfOutsideResources(keptPath);
            return AssetDatabase.LoadAssetAtPath<MenuIdentifierRegistry>(keptPath);
        }

        private static MenuIdentifierRegistry CreateRegistry()
        {
            string path = $"{DefaultRegistryDirectory}/{RegistryFileName}";
            Directory.CreateDirectory(DefaultRegistryDirectory);
            AssetDatabase.Refresh();

            MenuIdentifierRegistry registry = ScriptableObject.CreateInstance<MenuIdentifierRegistry>();
            AssetDatabase.CreateAsset(registry, path);

            CustomLogger.Log($"Created {nameof(MenuIdentifierRegistry)} at \"{path}\". "
                + "Move it anywhere under a Resources folder.", null);

            return registry;
        }

        /// <summary>
        /// The registry is loaded through <see cref="Resources"/> at runtime, so it is only included
        /// in a build when it sits under a Resources folder.
        /// </summary>
        private static void WarnIfOutsideResources(string path)
        {
            if (path.Contains(ResourcesFolder))
                return;

            CustomLogger.LogWarning($"{nameof(MenuIdentifierRegistry)} at \"{path}\" is not under a "
                + "Resources folder and will not be found in a build. Move it into one.", null);
        }

        private static void WriteAccessorClass(MenuIdentifier[] identifiers)
        {
            string contents = BuildAccessorSource(identifiers);
            Directory.CreateDirectory(OutputDirectory);

            // Skip writing if nothing changed, avoids triggering pointless recompiles.
            if (File.Exists(OutputPath) && File.ReadAllText(OutputPath) == contents)
            {
                AssetDatabase.SaveAssets();
                return;
            }

            File.WriteAllText(OutputPath, contents);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CustomLogger.Log($"Generated {GeneratedClassName} with {identifiers.Length} entries.", null);
        }

        private static string BuildAccessorSource(MenuIdentifier[] identifiers)
        {
            const string type = nameof(MenuIdentifier);
            const string loader = nameof(MenuIdentifierLoader);
            StringBuilder builder = new();

            builder.AppendLine("// <auto-generated>");
            builder.AppendLine("// This file is auto-generated. Do not edit manually.");
            builder.AppendLine($"// Regenerate via {RegenerateMenuPath.Replace("/", " > ")}.");
            builder.AppendLine("// </auto-generated>");
            builder.AppendLine();
            builder.AppendLine($"using {typeof(MenuIdentifier).Namespace};");
            builder.AppendLine();
            builder.AppendLine($"namespace {GeneratedNamespace}");
            builder.AppendLine("{");
            builder.AppendLine("    /// <summary>");
            builder.AppendLine($"    /// Provides strongly-typed access to all <see cref=\"{type}\"/> assets.");
            builder.AppendLine($"    /// References are resolved lazily via the <see cref=\"{loader}\"/>.");
            builder.AppendLine("    /// </summary>");
            builder.AppendLine($"    public static class {GeneratedClassName}");
            builder.AppendLine("    {");

            foreach (MenuIdentifier identifier in identifiers)
                AppendAccessor(builder, identifier);

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return builder.ToString();
        }

        private static void AppendAccessor(StringBuilder builder, MenuIdentifier identifier)
        {
            const string type = nameof(MenuIdentifier);
            string load = $"{nameof(MenuIdentifierLoader)}.{nameof(MenuIdentifierLoader.Load)}";
            string propertyName = SanitizeIdentifier(identifier.name);
            string fieldName = $"_{ToCamelCase(propertyName)}";

            builder.AppendLine($"        private static {type} {fieldName}; // reset-ignore");
            builder.AppendLine($"        public static {type} {propertyName} => {fieldName} != null");
            builder.AppendLine($"            ? {fieldName}");
            builder.AppendLine($"            : {fieldName} = {load}(\"{identifier.name}\");");
            builder.AppendLine();
        }

        private static string SanitizeAssetName(MenuIdentifier identifier) => SanitizeIdentifier(identifier.name);

        private static string SanitizeIdentifier(string name)
        {
            StringBuilder builder = new();

            foreach (char character in name)
            {
                if (char.IsLetterOrDigit(character) || character == '_')
                    builder.Append(character);
            }

            string result = builder.ToString();

            return result.Length == 0 || char.IsDigit(result[0])
                ? "_" + result
                : result;
        }

        private static string ToCamelCase(string value) => value.Length == 0
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }
}