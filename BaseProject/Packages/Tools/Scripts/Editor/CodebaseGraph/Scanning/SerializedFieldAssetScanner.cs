using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Counts how many assets set a serialized field. A field that Unity writes and no code reads is a
    /// very different story depending on the answer: nothing sets it either and it is plainly deletable,
    /// or five prefabs carry a value and it is a feature that was started and never finished.
    /// <br/><br/>
    /// Only text serialized assets can be read this way, and only fields already reported as never read
    /// are looked for, which keeps this to one pass over the project's YAML.
    /// </summary>
    public static class SerializedFieldAssetScanner
    {
        private const char KeyBoundary = ':';
        private const char LineBreak = '\n';
        private const string ProjectPrefix = "Assets/";
        private const string SceneExtension = ".unity";
        private const string ObjectExtension = ".asset";
        private const string PrefabExtension = ".prefab";
        private const string YamlHeader = "%YAML";

        /// <summary>Fills in how many assets set each field the analyzer flagged as never read.</summary>
        /// <param name="graph">Graph to annotate.</param>
        public static void Scan(CodebaseGraphData graph)
        {
            Dictionary<string, List<MemberNodeInfo>> byName = CollectCandidates(graph);
            if (byName.Count == 0)
                return;

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (!IsScannable(path))
                    continue;

                CountInAsset(path, byName);
            }
        }

        /// <summary>
        /// Pulls the YAML keys out of one asset. Searching the file once per candidate field would mean
        /// hundreds of full text scans of every prefab in the project, so the keys are read out in a
        /// single pass and matched against the candidates afterwards.
        /// </summary>
        private static void CollectKeys(string text, HashSet<string> keys)
        {
            keys.Clear();

            foreach (string line in text.Split(LineBreak))
            {
                int colon = line.IndexOf(KeyBoundary);
                if (colon <= 0)
                    continue;

                int start = 0;
                while (start < colon && line[start] == ' ')
                    start++;

                // A top level key is a document node rather than a serialized field.
                if (start == 0 || start >= colon)
                    continue;

                keys.Add(line[start..colon]);
            }
        }

        private static Dictionary<string, List<MemberNodeInfo>> CollectCandidates(CodebaseGraphData graph)
        {
            Dictionary<string, List<MemberNodeInfo>> byName = new(StringComparer.Ordinal);

            foreach (MemberNodeInfo member in graph.Members.Values)
            {
                if (!member.Issues.HasFlag(EMemberIssue.SerializedNeverRead))
                    continue;

                if (!byName.TryGetValue(member.Name, out List<MemberNodeInfo> list))
                {
                    list = new List<MemberNodeInfo>();
                    byName[member.Name] = list;
                }

                list.Add(member);
            }

            return byName;
        }

        /// <summary>Reused between assets so the scan allocates one set rather than one per file.</summary>
        private static readonly HashSet<string> Keys = new(StringComparer.Ordinal);

        private static bool IsScannable(string path)
            => path.StartsWith(ProjectPrefix, StringComparison.Ordinal)
                && (path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(ObjectExtension, StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase));

        private static void CountInAsset(string path, Dictionary<string, List<MemberNodeInfo>> byName)
        {
            string text = ReadText(path);
            if (string.IsNullOrEmpty(text) || !text.StartsWith(YamlHeader, StringComparison.Ordinal))
                return;

            CollectKeys(text, Keys);

            foreach (string key in Keys)
            {
                if (!byName.TryGetValue(key, out List<MemberNodeInfo> members))
                    continue;

                foreach (MemberNodeInfo member in members)
                    member.AssetUsageCount++;
            }
        }

        private static string ReadText(string path)
        {
            try
            {
                return File.Exists(path)
                    ? File.ReadAllText(path)
                    : string.Empty;
            }
            catch (Exception)
            {
                // A binary serialized or locked asset simply cannot answer the question.
                return string.Empty;
            }
        }
    }
}
