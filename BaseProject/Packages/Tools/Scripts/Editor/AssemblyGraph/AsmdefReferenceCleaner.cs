using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Removes unused references from asmdef files while keeping every other field intact.</summary>
    public static class AsmdefReferenceCleaner
    {
        /// <summary>
        /// Removes the given reference names from the asmdef at the given asset path.
        /// Returns how many references were removed.
        /// </summary>
        public static int RemoveReferences(string asmdefPath, HashSet<string> referenceNamesToRemove)
        {
            if (string.IsNullOrEmpty(asmdefPath) || referenceNamesToRemove == null || referenceNamesToRemove.Count == 0)
                return 0;

            string fullPath = ToAbsolutePath(asmdefPath);
            if (!File.Exists(fullPath))
                return 0;

            string text = File.ReadAllText(fullPath);
            MinimalAsmdef data = JsonUtility.FromJson<MinimalAsmdef>(text);
            if (data?.references == null || data.references.Length == 0)
                return 0;

            List<string> kept = new();
            int removed = 0;

            foreach (string rawToken in data.references)
            {
                string resolvedName = ResolveReferenceName(rawToken);
                if (referenceNamesToRemove.Contains(resolvedName))
                {
                    removed++;
                    continue;
                }

                kept.Add(rawToken);
            }

            if (removed == 0)
                return 0;

            string updated = ReplaceReferencesArray(text, kept);
            File.WriteAllText(fullPath, updated);
            AssetDatabase.ImportAsset(asmdefPath, ImportAssetOptions.ForceUpdate);
            return removed;
        }

        /// <summary>Turns a raw reference token (a name or a "GUID:xxxx" string) into an assembly name.</summary>
        private static string ResolveReferenceName(string rawToken)
        {
            if (string.IsNullOrEmpty(rawToken))
                return rawToken;

            const string guidPrefix = "GUID:";
            if (!rawToken.StartsWith(guidPrefix, StringComparison.Ordinal))
                return rawToken;

            string guid = rawToken.Substring(guidPrefix.Length);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return rawToken;

            try
            {
                MinimalAsmdef target = JsonUtility.FromJson<MinimalAsmdef>(File.ReadAllText(ToAbsolutePath(path)));
                return string.IsNullOrEmpty(target?.name)
                    ? rawToken
                    : target.name;
            }
            catch
            {
                return rawToken;
            }
        }

        /// <summary>Rewrites only the references array in the raw file text, preserving all other content.</summary>
        private static string ReplaceReferencesArray(string text, List<string> keptTokens)
        {
            int keyIndex = text.IndexOf("\"references\"", StringComparison.Ordinal);
            if (keyIndex < 0)
                return text;

            int open = text.IndexOf('[', keyIndex);
            int close = text.IndexOf(']', open);
            if (open < 0 || close < 0)
                return text;

            StringBuilder builder = new();
            builder.Append('[');

            if (keptTokens.Count > 0)
            {
                builder.Append('\n');
                for (int i = 0; i < keptTokens.Count; i++)
                {
                    builder.Append("        \"").Append(keptTokens[i]).Append('"');
                    if (i < keptTokens.Count - 1)
                        builder.Append(',');

                    builder.Append('\n');
                }

                builder.Append("    ");
            }

            builder.Append(']');

            return text.Substring(0, open) + builder + text.Substring(close + 1);
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, assetPath);
        }

        [Serializable]
        private sealed class MinimalAsmdef
        {
            public string name;
            public string[] references;
        }
    }
}