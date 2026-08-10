using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Checks stored dismissals against the graph. An id embeds the signature it was written for, so a
    /// rename correctly brings the finding back, but without this the dead entry sits in the file
    /// forever and nobody can tell a live decision from a fossil. Stale entries are reported, never
    /// deleted: a dismissal that stopped matching is exactly the moment a person should look.
    /// </summary>
    public static class DismissalAudit
    {
        private const char MemberBoundary = '#';
        private const char ParameterOpen = '(';
        private const string ReturnSeparator = " : ";

        /// <summary>Marks which entries no longer match anything, and what they may have become.</summary>
        /// <param name="graph">Graph to check against, or null when nothing has been scanned.</param>
        /// <param name="entries">Entries to annotate in place.</param>
        public static void Apply(CodebaseGraphData graph, List<DismissalEntry> entries)
        {
            if (graph == null)
                return;

            HashSet<string> known = new(StringComparer.Ordinal);
            Dictionary<string, List<string>> byMemberName = new(StringComparer.Ordinal);

            CollectKnown(graph, known, byMemberName);

            foreach (DismissalEntry entry in entries)
            {
                if (known.Contains(entry.Id))
                    continue;

                entry.IsStale = true;
                entry.SuggestedId = FindReplacement(entry, byMemberName);
            }
        }

        /// <summary>Counts the entries that no longer match anything.</summary>
        /// <param name="entries">Entries already annotated.</param>
        /// <returns>How many are stale.</returns>
        public static int CountStale(List<DismissalEntry> entries)
        {
            int count = 0;

            foreach (DismissalEntry entry in entries)
            {
                if (entry.IsStale)
                    count++;
            }

            return count;
        }

        private static void CollectKnown(CodebaseGraphData graph,
            HashSet<string> known,
            Dictionary<string, List<string>> byMemberName)
        {
            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
                known.Add(group.DismissalId);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                known.Add(type.DismissalId);

                foreach (MemberNodeInfo member in type.Members)
                {
                    known.Add(member.DismissalId);

                    string key = $"{type.FullName}{MemberBoundary}{member.Name}";
                    if (!byMemberName.TryGetValue(key, out List<string> ids))
                    {
                        ids = new List<string>();
                        byMemberName[key] = ids;
                    }

                    ids.Add(member.DismissalId);
                }
            }
        }

        /// <summary>
        /// A member that kept its type and its name but lost its id changed signature, which is a very
        /// different event from being deleted. Only an unambiguous single match is offered.
        /// </summary>
        private static string FindReplacement(DismissalEntry entry, Dictionary<string, List<string>> byMemberName)
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

            return ids[0];
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
