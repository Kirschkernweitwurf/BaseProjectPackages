using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Base.ToolPackage.Editor.OverviewGui.UnusedScriptsOverviewWindow
{
    /// <summary>
    /// Finds scripts that look dead. A script counts as alive when a scene, prefab, or asset
    /// references it, when any other script mentions one of its type names as a whole word
    /// (this catches inheritance, AddComponent, nameof, and string or reflection lookups), or
    /// when it belongs to an editor assembly and editor scripts are ignored. This is a heuristic,
    /// so review before deleting: fully dynamic reflection and code generation cannot be detected.
    /// </summary>
    public static class UnusedScriptScanner
    {
        private const string AttributeSuffix = "Attribute";

        private static readonly Regex TypeDeclRegex =
            new(@"\b(?:class|struct|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex IdentifierRegex = new(@"[A-Za-z_][A-Za-z0-9_]*", RegexOptions.Compiled);

        public static List<UnusedScriptEntry> Scan(bool ignoreEditorScripts)
        {
            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);

            List<string> scripts = allPaths
                .Where(path => path.StartsWith("Assets/") && path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .ToList();

            HashSet<string> assetReferenced = CollectAssetReferencedScripts(allPaths);
            HashSet<string> editorScripts = CollectEditorScripts();

            Dictionary<string, List<string>> candidateNames = new();
            HashSet<string> targetNames = new();

            foreach (string script in scripts)
            {
                if (ignoreEditorScripts && editorScripts.Contains(script))
                    continue;

                List<string> names = ExtractDeclaredNames(projectPath + script);

                if (names.Count == 0)
                    continue;

                candidateNames[script] = names;
                targetNames.UnionWith(names);
            }

            Dictionary<string, HashSet<string>> usage = BuildUsage(scripts, projectPath, targetNames);

            List<UnusedScriptEntry> results = new();

            foreach (KeyValuePair<string, List<string>> candidate in candidateNames)
            {
                string path = candidate.Key;

                if (assetReferenced.Contains(path) || IsUsedInCode(path, candidate.Value, usage))
                    continue;

                results.Add(new UnusedScriptEntry(path, AssetDatabase.AssetPathToGUID(path)));
            }

            results.Sort((first, second) => string.Compare(first.Path, second.Path, StringComparison.Ordinal));
            return results;
        }

        private static Dictionary<string, HashSet<string>> BuildUsage(List<string> scripts, string projectPath,
            HashSet<string> targetNames)
        {
            Dictionary<string, HashSet<string>> usage = new();

            try
            {
                for (int i = 0; i < scripts.Count; i++)
                {
                    string script = scripts[i];

                    EditorUtility.DisplayProgressBar("Scanning Scripts", script,
                        (float)i / Mathf.Max(1, scripts.Count));

                    HashSet<string> tokens = Tokenize(projectPath + script);

                    foreach (string name in targetNames)
                    {
                        if (!tokens.Contains(name))
                            continue;

                        if (!usage.TryGetValue(name, out HashSet<string> files))
                        {
                            files = new HashSet<string>();
                            usage[name] = files;
                        }

                        files.Add(script);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return usage;
        }

        private static bool IsUsedInCode(string path, List<string> names, Dictionary<string, HashSet<string>> usage)
        {
            foreach (string name in names)
            {
                if (!usage.TryGetValue(name, out HashSet<string> files))
                    continue;

                foreach (string file in files)
                {
                    if (file != path)
                        return true;
                }
            }

            return false;
        }

        private static HashSet<string> CollectAssetReferencedScripts(string[] allPaths)
        {
            string[] roots = allPaths
                .Where(path => path.StartsWith("Assets/")
                    && (
                        path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                        || path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                        || path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            return AssetDatabase.GetDependencies(roots, true)
                .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .ToHashSet();
        }

        private static HashSet<string> CollectEditorScripts()
        {
            HashSet<string> editorScripts = new();

            foreach (Assembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                if (!assembly.flags.HasFlag(AssemblyFlags.EditorAssembly))
                    continue;

                foreach (string source in assembly.sourceFiles)
                    editorScripts.Add(source);
            }

            return editorScripts;
        }

        private static List<string> ExtractDeclaredNames(string absolutePath)
        {
            string text = ReadText(absolutePath);
            List<string> names = new();

            foreach (Match match in TypeDeclRegex.Matches(text))
            {
                string name = match.Groups[1].Value;
                names.Add(name);

                // Attributes are used without the suffix, for example [Foo] for class FooAttribute.
                if (name.EndsWith(AttributeSuffix, StringComparison.Ordinal) && name.Length > AttributeSuffix.Length)
                    names.Add(name.Substring(0, name.Length - AttributeSuffix.Length));
            }

            return names.Distinct().ToList();
        }

        private static HashSet<string> Tokenize(string absolutePath)
        {
            HashSet<string> tokens = new();

            foreach (Match match in IdentifierRegex.Matches(ReadText(absolutePath)))
                tokens.Add(match.Value);

            return tokens;
        }

        private static string ReadText(string absolutePath)
        {
            try
            {
                return File.ReadAllText(absolutePath);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}