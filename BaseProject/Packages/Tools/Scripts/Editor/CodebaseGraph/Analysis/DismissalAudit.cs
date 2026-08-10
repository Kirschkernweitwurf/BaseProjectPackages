using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Checks stored dismissals against the graph. An id embeds both the signature and the finding it
    /// was written for, so a rename or a fix correctly brings the finding back, but without this the
    /// dead entry sits in the file forever and nobody can tell a live decision from a fossil.
    /// <br/><br/>
    /// The two ways an entry can stop matching are kept apart on purpose. An entity that no longer
    /// exists is dead configuration. An entity that still exists whose finding no longer fires means
    /// either the problem was fixed or a rule stopped detecting something it used to catch, and that
    /// second case is the only warning anyone would ever get that a check went quietly inert.
    /// <br/><br/>
    /// Nothing is deleted here. A dismissal that stopped matching is exactly the moment a person
    /// should look at it.
    /// </summary>
    public static class DismissalAudit
    {
        private const char MemberBoundary = '#';
        private const char ParameterOpen = '(';
        private const string ReturnSeparator = " : ";

        /// <summary>Marks which entries no longer match anything, why and what they may have become.</summary>
        /// <param name="graph">Graph to check against, or null when nothing has been scanned.</param>
        /// <param name="entries">Entries to annotate in place.</param>
        public static void Apply(CodebaseGraphData graph, List<DismissalEntry> entries)
        {
            if (graph == null)
                return;

            HashSet<string> entities = new(StringComparer.Ordinal);
            HashSet<string> reported = new(StringComparer.Ordinal);
            Dictionary<string, List<string>> byMemberName = new(StringComparer.Ordinal);

            CollectKnown(graph, entities, reported, byMemberName);

            foreach (DismissalEntry entry in entries)
                Inspect(entry, entities, reported, byMemberName);
        }

        /// <summary>Counts the entries that stopped matching for one reason.</summary>
        /// <param name="entries">Entries already annotated.</param>
        /// <param name="reason">Reason to count.</param>
        /// <returns>How many carry that reason.</returns>
        public static int Count(List<DismissalEntry> entries, EStaleReason reason)
        {
            int count = 0;

            foreach (DismissalEntry entry in entries)
            {
                if (entry.StaleReason == reason)
                    count++;
            }

            return count;
        }

        /// <summary>Counts every entry that stopped matching, whatever the reason.</summary>
        /// <param name="entries">Entries already annotated.</param>
        /// <returns>How many are stale.</returns>
        public static int CountStale(List<DismissalEntry> entries)
            => Count(entries, EStaleReason.Missing) + Count(entries, EStaleReason.Resolved);

        private static void Inspect(DismissalEntry entry,
            HashSet<string> entities,
            HashSet<string> reported,
            Dictionary<string, List<string>> byMemberName)
        {
            string entity = GraphIdentity.ReadEntry(entry.Id, out EFinding finding);

            if (!entities.Contains(entity))
            {
                entry.StaleReason = EStaleReason.Missing;
                entry.SuggestedId = FindReplacement(entry, finding, byMemberName);
                return;
            }

            // The entity is still there, so an id naming a finding that is no longer raised means the
            // problem went away rather than the code did.
            if (finding != EFinding.None && !reported.Contains(entry.Id))
                entry.StaleReason = EStaleReason.Resolved;
        }

        private static void CollectKnown(CodebaseGraphData graph,
            HashSet<string> entities,
            HashSet<string> reported,
            Dictionary<string, List<string>> byMemberName)
        {
            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
            {
                entities.Add(group.DismissalId);

                if (group.CyclePartners.Count > 0)
                    reported.Add(GraphIdentity.ForFinding(group.DismissalId, EFinding.NamespaceCycle));
            }

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                entities.Add(type.DismissalId);

                foreach (EFinding finding in FindingCatalog.ReadReported(type))
                    reported.Add(GraphIdentity.ForFinding(type.DismissalId, finding));

                CollectMembers(type, entities, reported, byMemberName);
            }
        }

        private static void CollectMembers(TypeNodeInfo type,
            HashSet<string> entities,
            HashSet<string> reported,
            Dictionary<string, List<string>> byMemberName)
        {
            foreach (MemberNodeInfo member in type.Members)
            {
                entities.Add(member.DismissalId);

                foreach (EFinding finding in FindingCatalog.ReadReported(member))
                    reported.Add(GraphIdentity.ForFinding(member.DismissalId, finding));

                string key = $"{type.FullName}{MemberBoundary}{member.Name}";
                if (!byMemberName.TryGetValue(key, out List<string> ids))
                {
                    ids = new List<string>();
                    byMemberName[key] = ids;
                }

                ids.Add(member.DismissalId);
            }
        }

        /// <summary>
        /// A member that kept its type and its name but lost its id changed signature, which is a very
        /// different event from being deleted. Only an unambiguous single match is offered.
        /// </summary>
        private static string FindReplacement(DismissalEntry entry,
            EFinding finding,
            Dictionary<string, List<string>> byMemberName)
        {
            if (entry.Kind != EDismissalKind.Member)
                return null;

            int boundary = entry.DisplayName.IndexOf(MemberBoundary);
            if (boundary <= 0)
                return null;

            string owner = entry.DisplayName[..boundary];
            string signature = entry.DisplayName[(boundary + 1)..];
            string key = $"{owner}{MemberBoundary}{ReadMemberName(signature)}";

            if (!byMemberName.TryGetValue(key, out List<string> ids) || ids.Count != 1)
                return null;

            return finding == EFinding.None
                ? ids[0]
                : GraphIdentity.ForFinding(ids[0], finding);
        }

        private static string ReadMemberName(string signature)
        {
            int cut = signature.Length;

            int parameters = signature.IndexOf(ParameterOpen);
            if (parameters >= 0)
                cut = parameters;

            int returns = signature.IndexOf(ReturnSeparator, StringComparison.Ordinal);
            if (returns >= 0 && returns < cut)
                cut = returns;

            return signature[..cut].Trim();
        }
    }
}
