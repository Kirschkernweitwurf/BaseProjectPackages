using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.OverviewGui.UnusedAssetsOverviewWindow
{
    /// <summary>
    /// Finds assets that look unused. An asset counts as used when it is reachable from an enabled
    /// build scene, any Resources folder, the render pipeline, preloaded assets, anything referenced
    /// from ProjectSettings, or an Addressables entry. Everything else under Assets, minus code and
    /// editor files, is reported. This is a heuristic, so review before deleting: assets loaded only
    /// by code or by string path cannot be detected.
    /// </summary>
    public static class UnusedAssetScanner
    {
        private static readonly string[] CodeExtensions =
        {
            ".cs",
            ".asmdef",
            ".asmref",
            ".dll",
            ".rsp"
        };
        private static readonly Regex GuidRegex = new(@"guid:\s*([0-9a-fA-F]{32})", RegexOptions.Compiled);

        public static List<UnusedAssetEntry> Scan(bool ignoreEditorFolders)
        {
            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

            HashSet<string> roots = CollectRoots(allPaths, projectPath);
            HashSet<string> used = new(AssetDatabase.GetDependencies(roots.ToArray(), true));
            used.UnionWith(roots);

            List<UnusedAssetEntry> results = new();

            try
            {
                for (int i = 0; i < allPaths.Length; i++)
                {
                    string path = allPaths[i];

                    EditorUtility.DisplayProgressBar("Scanning Assets", path, (float)i / Mathf.Max(1, allPaths.Length));

                    if (!path.StartsWith("Assets/") || AssetDatabase.IsValidFolder(path))
                        continue;

                    if (used.Contains(path) || IsExcluded(path, ignoreEditorFolders))
                        continue;

                    Type type = AssetDatabase.GetMainAssetTypeAtPath(path);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    long size = GetFileSize(projectPath + path);

                    results.Add(new UnusedAssetEntry(path, guid, type != null
                        ? type.Name
                        : "Unknown", size));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            results.Sort(Compare);
            return results;
        }

        private static HashSet<string> CollectRoots(string[] allPaths, string projectPath)
        {
            HashSet<string> roots = new();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                    roots.Add(scene.path);
            }

            foreach (string path in allPaths)
            {
                if (path.Contains("/Resources/") && !AssetDatabase.IsValidFolder(path))
                    roots.Add(path);
            }

            CollectRenderPipeline(roots);
            CollectPreloadedAssets(roots);
            CollectProjectSettings(roots, projectPath);
            CollectAddressables(roots);
            return roots;
        }

        private static void CollectRenderPipeline(HashSet<string> roots)
        {
            AddObject(roots, GraphicsSettings.defaultRenderPipeline);
            AddObject(roots, QualitySettings.renderPipeline);
        }

        private static void CollectPreloadedAssets(HashSet<string> roots)
        {
            foreach (Object asset in PlayerSettings.GetPreloadedAssets())
                AddObject(roots, asset);
        }

        // Reads GUID references out of ProjectSettings so render pipeline, quality, input, and
        // localization config assets are treated as used. Works when assets serialize as text.
        private static void CollectProjectSettings(HashSet<string> roots, string projectPath)
        {
            string directory = projectPath + "ProjectSettings";

            if (!Directory.Exists(directory))
                return;

            foreach (string file in Directory.GetFiles(directory, "*.asset"))
            {
                string text;

                try
                {
                    text = File.ReadAllText(file);
                }
                catch
                {
                    continue;
                }

                foreach (Match match in GuidRegex.Matches(text))
                {
                    string path = AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);

                    if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets/"))
                        roots.Add(path);
                }
            }
        }

        // Reflection based, so the tool compiles whether or not the Addressables package is present.
        private static void CollectAddressables(HashSet<string> roots)
        {
            Type defaultObject = FindType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject");
            object settings = defaultObject?.GetProperty("Settings", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);

            if (settings == null)
                return;

            if (!(settings.GetType().GetProperty("groups")?.GetValue(settings) is IEnumerable groups))
                return;

            foreach (object group in groups)
            {
                if (!(group?.GetType().GetProperty("entries")?.GetValue(group) is IEnumerable entries))
                    continue;

                foreach (object entry in entries)
                {
                    string path = entry?.GetType().GetProperty("AssetPath")?.GetValue(entry) as string;

                    if (string.IsNullOrEmpty(path))
                        continue;

                    if (AssetDatabase.IsValidFolder(path))
                        foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[]
                                 {
                                     path
                                 }))
                            roots.Add(AssetDatabase.GUIDToAssetPath(guid));
                    else
                        roots.Add(path);
                }
            }
        }

        private static void AddObject(HashSet<string> roots, Object asset)
        {
            if (asset == null)
                return;

            string path = AssetDatabase.GetAssetPath(asset);

            if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets/"))
                roots.Add(path);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);

                if (type != null)
                    return type;
            }

            return null;
        }

        private static bool IsExcluded(string path, bool ignoreEditorFolders)
        {
            foreach (string extension in CodeExtensions)
            {
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            if (path.Contains("/Editor Default Resources/") || path.Contains("/Gizmos/"))
                return true;

            if (path.Contains("AddressableAssetsData"))
                return true;

            if (ignoreEditorFolders && path.Contains("/Editor/"))
                return true;

            return false;
        }

        private static long GetFileSize(string absolutePath)
        {
            try
            {
                return new FileInfo(absolutePath).Length;
            }
            catch
            {
                return 0L;
            }
        }

        private static int Compare(UnusedAssetEntry first, UnusedAssetEntry second)
        {
            int byType = string.Compare(first.TypeName, second.TypeName, StringComparison.Ordinal);
            return byType != 0
                ? byType
                : string.Compare(first.Path, second.Path, StringComparison.Ordinal);
        }
    }
}