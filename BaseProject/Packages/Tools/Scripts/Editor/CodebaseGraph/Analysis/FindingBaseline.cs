using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Works out what a scan found that the one before it did not. Most of a findings list is the same
    /// list you already read and decided about, and the part worth looking at is what changed since you
    /// last looked, which is otherwise invisible in a list of four hundred.
    /// <br/><br/>
    /// Nothing is stored on disk. The comparison is against the previous scan in this session, so the
    /// first scan after opening Unity has nothing to compare against and marks nothing new, which is
    /// honest: with no baseline the only truthful answer is that everything is equally old.
    /// </summary>
    public static class FindingBaseline
    {
        /// <summary>Collects the id of every finding currently raised.</summary>
        /// <param name="graph">Graph to read.</param>
        /// <returns>The ids, in the same form dismissals use.</returns>
        public static HashSet<string> Collect(CodebaseGraphData graph)
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
        public static void Apply(CodebaseGraphData graph, HashSet<string> previous)
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
