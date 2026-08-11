using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.FolderConventionValidator
{
    /// <summary>
    /// Walks the folders below the configured root and reports every rule of a
    /// <see cref="FolderConventionConfig"/> that is broken.
    /// </summary>
    internal static class FolderConventionScanner
    {
        private const string AssetSearchFilter = "t:Object";
        private const string AssetsRoot = "Assets";
        private const string PackagesRoot = "Packages";
        private const char PathSeparator = '/';

        private static readonly Regex CamelCase = new("^[a-z][a-zA-Z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex KebabCase = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);
        private static readonly Regex PascalCase = new("^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex SnakeCase = new("^[a-z0-9]+(_[a-z0-9]+)*$", RegexOptions.Compiled);

        /// <summary>Scans the project and returns every violation, sorted by path.</summary>
        public static List<FolderViolation> Scan(FolderConventionConfig config)
        {
            List<FolderViolation> violations = new();

            if (config == null)
            {
                CustomLogger.LogError($"Scanning needs a {nameof(FolderConventionConfig)}.", null);
                return violations;
            }

            string root = Normalize(config.RootFolder);

            if (!AssetDatabase.IsValidFolder(root))
            {
                CustomLogger.LogWarning($"The root folder \"{root}\" does not exist.", config);
                return violations;
            }

            CollectMissingFolders(config, violations);
            CollectLooseAssets(config, root, violations);
            CollectFolderViolations(config, root, 0, violations);

            violations.Sort(Compare);
            return violations;
        }

        /// <summary>Creates the folder and all missing parents. True when it exists afterward.</summary>
        public static bool CreateFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                CustomLogger.LogWarning("Cannot create a folder without a path.", null);
                return false;
            }

            string normalized = Normalize(path);

            if (AssetDatabase.IsValidFolder(normalized))
                return true;

            string[] segments = normalized.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            // A folder always lives below Assets or Packages, so a single segment cannot be created.
            if (segments.Length < 2)
            {
                CustomLogger.LogError($"\"{path}\" is not a valid folder path. It has to start with "
                    + $"{AssetsRoot} or {PackagesRoot}.", null);

                return false;
            }

            string current = segments[0];

            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}{PathSeparator}{segments[index]}";

                if (!AssetDatabase.IsValidFolder(next)
                    && !IsCreated(current, segments[index]))
                    return false;

                current = next;
            }

            AssetDatabase.Refresh();
            return AssetDatabase.IsValidFolder(normalized);
        }

        private static void CollectMissingFolders(FolderConventionConfig config, List<FolderViolation> violations)
        {
            foreach (string entry in config.RequiredFolders)
            {
                if (string.IsNullOrWhiteSpace(entry))
                    continue;

                string path = Normalize(entry);

                if (AssetDatabase.IsValidFolder(path))
                    continue;

                violations.Add(new FolderViolation(EFolderViolationType.MissingFolder, path,
                    "Required folder is missing."));
            }
        }

        private static void CollectLooseAssets(FolderConventionConfig config, string root,
            List<FolderViolation> violations)
        {
            if (config.AllowLooseAssetsInRoot)
                return;

            foreach (string guid in AssetDatabase.FindAssets(AssetSearchFilter, new[]
                     {
                         root
                     }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path))
                    continue;

                if (GetParentFolder(path) != root)
                    continue;

                violations.Add(new FolderViolation(EFolderViolationType.LooseAsset, path,
                    $"Asset sits directly in \"{root}\" instead of a subfolder."));
            }
        }

        // Depth is counted from the root, so a direct child sits at level one.
        private static void CollectFolderViolations(FolderConventionConfig config, string folder, int depth,
            List<FolderViolation> violations)
        {
            foreach (string subFolder in AssetDatabase.GetSubFolders(folder))
            {
                string name = Path.GetFileName(subFolder);

                if (IsListed(config.IgnoredFolders, name))
                    continue;

                int childDepth = depth + 1;
                CollectNameViolations(config, subFolder, name, violations);
                CollectDepthViolation(config, subFolder, childDepth, violations);
                CollectFolderViolations(config, subFolder, childDepth, violations);
            }
        }

        private static void CollectNameViolations(FolderConventionConfig config, string folder, string name,
            List<FolderViolation> violations)
        {
            if (IsListed(config.ForbiddenNames, name))
            {
                violations.Add(new FolderViolation(EFolderViolationType.ForbiddenName, folder,
                    $"The name \"{name}\" is on the forbidden list."));

                return;
            }

            if (IsListed(config.AllowedNameExceptions, name))
                return;

            if (IsValidName(name, config.NamingStyle))
                return;

            violations.Add(new FolderViolation(EFolderViolationType.NamingStyle, folder,
                $"The name \"{name}\" does not match {config.NamingStyle}."));
        }

        // Only the first level past the limit is reported, otherwise every nested folder shows up.
        private static void CollectDepthViolation(FolderConventionConfig config, string folder, int depth,
            List<FolderViolation> violations)
        {
            if (depth != config.MaxDepth + 1)
                return;

            violations.Add(new FolderViolation(EFolderViolationType.ExceededDepth, folder,
                $"Nested {depth} levels deep, the limit is {config.MaxDepth}."));
        }

        private static bool IsCreated(string parent, string name)
        {
            string guid = AssetDatabase.CreateFolder(parent, name);

            if (!string.IsNullOrEmpty(guid))
                return true;

            CustomLogger.LogError($"Could not create \"{name}\" inside \"{parent}\".", null);
            return false;
        }

        private static bool IsValidName(string name, EFolderNamingStyle style) => style switch
        {
            EFolderNamingStyle.PascalCase => PascalCase.IsMatch(name),
            EFolderNamingStyle.CamelCase => CamelCase.IsMatch(name),
            EFolderNamingStyle.SnakeCase => SnakeCase.IsMatch(name),
            EFolderNamingStyle.KebabCase => KebabCase.IsMatch(name),
            _ => true
        };

        private static bool IsListed(List<string> names, string name)
        {
            foreach (string entry in names)
            {
                if (string.Equals(entry, name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string GetParentFolder(string path)
        {
            int slash = path.LastIndexOf(PathSeparator);

            return slash <= 0
                ? string.Empty
                : path[..slash];
        }

        // Accepts backslashes and paths written without the Assets prefix, so hand typed
        // entries in the config still resolve to a real asset path.
        private static string Normalize(string path)
        {
            string cleaned = path
                .Replace('\\', PathSeparator)
                .Trim()
                .Trim(PathSeparator);

            if (cleaned.Length == 0)
                return cleaned;

            string root = cleaned.Split(PathSeparator)[0];

            bool hasKnownRoot = root == AssetsRoot
                || root == PackagesRoot;

            return hasKnownRoot
                ? cleaned
                : $"{AssetsRoot}{PathSeparator}{cleaned}";
        }

        private static int Compare(FolderViolation first, FolderViolation second)
        {
            int byPath = string.Compare(first.Path, second.Path, StringComparison.Ordinal);

            return byPath != 0
                ? byPath
                : first.Type.CompareTo(second.Type);
        }
    }
}