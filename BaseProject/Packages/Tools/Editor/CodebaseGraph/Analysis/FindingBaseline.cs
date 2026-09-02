using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Works out what a scan found that the one before it did not. Most of a findings list is the same
    /// list you already read and decided about, and the part worth looking at is what changed since you
    /// last looked, which is otherwise invisible in a list of four hundred.
    /// <br/><br/>
    /// The ids are kept on disk beside the dismissals. They have to be: fixing something recompiles,
    /// and a baseline held in memory dies with the domain, so the scan that comes after a fix would
    /// always find nothing to compare against. That is the one moment this exists for.
    /// </summary>
    internal static class FindingBaseline
    {
        private const string FilePath = "ProjectSettings/CodebaseGraphBaseline.json";

        /// <summary>Reads the ids the previous scan wrote.</summary>
        /// <returns>The ids, or an empty set when there is no baseline yet.</returns>
        internal static HashSet<string> Read()
        {
            if (!File.Exists(FilePath))
                return new HashSet<string>();

            try
            {
                FindingBaselineData data =
                    JsonUtility.FromJson<FindingBaselineData>(File.ReadAllText(FilePath));

                return data == null
                    ? new HashSet<string>()
                    : new HashSet<string>(data.Ids);
            }
            catch (Exception exception)
            {
                CustomLogger.LogWarning($"Could not read {FilePath}: {exception.Message}", null);
                return new HashSet<string>();
            }
        }

        /// <summary>Writes the ids this scan raised, to compare the next one against.</summary>
        /// <param name="ids">Ids the scan raised.</param>
        internal static void Write(HashSet<string> ids)
        {
            FindingBaselineData data = new();
            data.Ids.AddRange(ids);

            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception)
            {
                // A read only file under source control throws UnauthorizedAccessException rather than
                // IOException, and losing a baseline must not take the scan down with it.
                CustomLogger.LogWarning($"Could not write {FilePath}: {exception.Message}", null);
            }
        }

        /// <summary>Collects the id of every finding currently raised.</summary>
        /// <param name="graph">Graph to read.</param>
        /// <returns>The ids, in the same form dismissals use.</returns>
        internal static HashSet<string> Collect(CodebaseGraphData graph)
        {
            HashSet<string> ids = new();

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
            {
                if (group.CyclePartners.Count > 0)
                    ids.Add(GraphIdentity.ForFinding(group.DismissalId, EFinding.NamespaceCycle));
            }

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                foreach (EFinding finding in FindingCatalog.ReadReported(type))
                    ids.Add(GraphIdentity.ForFinding(type.DismissalId, finding));

                foreach (MemberNodeInfo member in type.Members)
                {
                    foreach (EFinding finding in FindingCatalog.ReadReported(member))
                        ids.Add(GraphIdentity.ForFinding(member.DismissalId, finding));
                }
            }

            return ids;
        }

        /// <summary>Marks everything raised now that was not raised before.</summary>
        /// <param name="graph">Graph to annotate.</param>
        /// <param name="previous">Ids the previous scan raised, or null when there was none.</param>
        internal static void Apply(CodebaseGraphData graph, HashSet<string> previous)
        {
            if (previous == null || previous.Count == 0)
                return;

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
            {
                group.HasNewFindings = group.CyclePartners.Count > 0
                    && !previous.Contains(GraphIdentity.ForFinding(group.DismissalId,
                        EFinding.NamespaceCycle));
            }

            foreach (TypeNodeInfo type in graph.Types.Values)
                ApplyToType(type, previous);
        }

        private static void ApplyToType(TypeNodeInfo type, HashSet<string> previous)
        {
            foreach (EFinding finding in FindingCatalog.ReadReported(type))
            {
                if (previous.Contains(GraphIdentity.ForFinding(type.DismissalId, finding)))
                    continue;

                type.HasNewFindings = true;
                break;
            }

            foreach (MemberNodeInfo member in type.Members)
            {
                foreach (EFinding finding in FindingCatalog.ReadReported(member))
                {
                    if (previous.Contains(GraphIdentity.ForFinding(member.DismissalId, finding)))
                        continue;

                    member.HasNewFindings = true;
                    break;
                }
            }
        }
    }
}