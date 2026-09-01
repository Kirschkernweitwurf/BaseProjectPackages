using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reads what the assets say about the code. Four things live only in YAML and nowhere in IL: which
    /// serialized fields actually carry a value, which methods an inspector wired UnityEvent calls,
    /// which methods an animation clip calls, and which types were stored by SerializeReference.
    /// <br/><br/>
    /// Fields are tied to the script that owns the document they appear in. Matching a field name across
    /// the whole project would credit a dead field called speed with every prefab that has any speed on
    /// any component, and that count feeds the ranking, so a loose match quietly degrades the one
    /// finding with the best evidence behind it.
    /// </summary>
    internal static class SerializedFieldAssetScanner
    {
        private const string AnimationExtension = ".anim";
        private const string ControllerExtension = ".controller";
        private const string DocumentMarker = "---";
        private const string FunctionNameKey = "functionName:";
        private const string GuidPattern = @"guid:\s*([0-9a-fA-F]{32})";
        private const char KeyBoundary = ':';
        private const char LineBreak = '\n';
        private const string MethodNameKey = "m_MethodName:";
        private const string ObjectExtension = ".asset";
        private const string PackagePrefix = "Packages/";
        private const string PrefabExtension = ".prefab";
        private const string ProjectPrefix = "Assets/";
        private const string ReferenceTypePattern = @"class:\s*([\w`]+),\s*ns:\s*([\w\.]*),";
        private const string SceneExtension = ".unity";
        private const string ScriptKey = "m_Script:";
        private const string TargetTypeKey = "m_TargetAssemblyTypeName:";
        private const string YamlHeader = "%YAML";

        private static readonly Regex GuidRegex = new(GuidPattern, RegexOptions.Compiled);
        private static readonly Regex ReferenceTypeRegex = new(ReferenceTypePattern, RegexOptions.Compiled);

        /// <summary>Reads every text serialized asset and credits what it finds back to the graph.</summary>
        /// <param name="graph">Graph to annotate.</param>
        /// <param name="progress">Reporter that can also cancel the pass.</param>
        /// <returns>False when the scan was cancelled.</returns>
        internal static bool Scan(CodebaseGraphData graph, ScanProgress progress)
        {
            AssetScanContext context = AssetScanContext.Build(graph);
            if (context.IsEmpty)
                return true;

            List<string> paths = CollectPaths();

            for (int index = 0; index < paths.Count; index++)
            {
                ReadAsset(paths[index], context);

                if (!progress.Report(index, paths.Count, "Reading prefabs and scenes"))
                    return false;
            }

            return true;
        }

        private static List<string> CollectPaths()
        {
            List<string> paths = new();

            foreach (string path in AssetDatabase.GetAllAssetPaths())
            {
                if (IsScannable(path))
                    paths.Add(path);
            }

            return paths;
        }

        private static bool IsScannable(string path)
        {
            // Packages count too: the asset that motivated reading SerializeReference entries at all
            // lives in one, so restricting this to the project folder would miss the case outright.
            if (!path.StartsWith(ProjectPrefix, StringComparison.Ordinal)
                && !path.StartsWith(PackagePrefix, StringComparison.Ordinal))
                return false;

            return path.EndsWith(PrefabExtension, StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(ObjectExtension, StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(SceneExtension, StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(AnimationExtension, StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(ControllerExtension, StringComparison.OrdinalIgnoreCase);
        }

        private static void ReadAsset(string path, AssetScanContext context)
        {
            string text = ReadText(path);
            if (string.IsNullOrEmpty(text) || !text.StartsWith(YamlHeader, StringComparison.Ordinal))
                return;

            TypeNodeInfo owner = null;
            bool hasScript = false;
            string eventTarget = null;
            HashSet<string> credited = new(StringComparer.Ordinal);

            foreach (string line in text.Split(LineBreak))
            {
                if (line.StartsWith(DocumentMarker, StringComparison.Ordinal))
                {
                    owner = null;
                    hasScript = false;
                    eventTarget = null;
                    credited.Clear();
                    continue;
                }

                if (TryReadOwner(line, context, ref owner, ref hasScript))
                    continue;

                if (TryReadEventTarget(line, ref eventTarget))
                    continue;

                if (TryReadEventMethod(line, eventTarget, context))
                    continue;

                if (TryReadAnimationEvent(line, context))
                    continue;

                if (TryReadReferenceType(line, context))
                    continue;

                if (hasScript)
                    CreditField(line, owner, context, credited);
            }
        }

        /// <summary>
        /// Reads which script a document belongs to. A document with no script at all credits nothing,
        /// but one whose script cannot be resolved still does, by name alone, because a generic
        /// MonoBehaviour resolves to nothing and refusing there would promote its fields to the top of
        /// the report as things to delete.
        /// </summary>
        private static bool TryReadOwner(string line,
            AssetScanContext context,
            ref TypeNodeInfo owner,
            ref bool hasScript)
        {
            if (line.IndexOf(ScriptKey, StringComparison.Ordinal) < 0)
                return false;

            Match match = GuidRegex.Match(line);
            owner = match.Success
                ? context.ResolveByGuid(match.Groups[1].Value)
                : null;

            hasScript = true;
            return true;
        }

        private static bool TryReadAnimationEvent(string line, AssetScanContext context)
        {
            int marker = line.IndexOf(FunctionNameKey, StringComparison.Ordinal);
            if (marker < 0)
                return false;

            string method = line[(marker + FunctionNameKey.Length)..].Trim();
            if (method.Length > 0)
                context.MarkAnimationEvent(method);

            return true;
        }

        private static bool TryReadEventTarget(string line, ref string eventTarget)
        {
            int marker = line.IndexOf(TargetTypeKey, StringComparison.Ordinal);
            if (marker < 0)
                return false;

            string value = line[(marker + TargetTypeKey.Length)..].Trim();
            int assembly = value.IndexOf(',');

            eventTarget = assembly < 0
                ? value
                : value[..assembly].Trim();

            return true;
        }

        private static bool TryReadEventMethod(string line, string eventTarget, AssetScanContext context)
        {
            int marker = line.IndexOf(MethodNameKey, StringComparison.Ordinal);
            if (marker < 0)
                return false;

            if (string.IsNullOrEmpty(eventTarget))
                return true;

            string method = line[(marker + MethodNameKey.Length)..].Trim();
            context.MarkEventTarget(eventTarget, method);

            return true;
        }

        private static bool TryReadReferenceType(string line, AssetScanContext context)
        {
            Match match = ReferenceTypeRegex.Match(line);
            if (!match.Success)
                return false;

            string space = match.Groups[2].Value;
            string name = match.Groups[1].Value;

            context.MarkReferenceType(string.IsNullOrEmpty(space)
                ? name
                : $"{space}.{name}");

            return true;
        }

        private static void CreditField(string line,
            TypeNodeInfo owner,
            AssetScanContext context,
            HashSet<string> credited)
        {
            int colon = line.IndexOf(KeyBoundary);
            if (colon <= 0)
                return;

            int start = 0;
            while (start < colon && line[start] == ' ')
                start++;

            // A top level key is a document node rather than a serialized field.
            if (start == 0 || start >= colon)
                return;

            string key = line[start..colon];
            if (!credited.Add(key))
                return;

            context.CreditField(owner, key);
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