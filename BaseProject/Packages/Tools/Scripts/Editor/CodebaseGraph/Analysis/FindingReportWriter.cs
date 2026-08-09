using System;
using System.Collections.Generic;
using System.Text;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.Editor.CodebaseGraph.Scanning;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Writes the findings out as Markdown, aimed at a coding agent: one section per kind of finding
    /// with the explanation stated once, then a flat list of dismissal ids with file paths and line
    /// numbers it can open directly, and the caveats up front so it does not delete something Unity
    /// calls by name.
    /// <br/><br/>
    /// Findings are ranked and split. A scan of a real project produces thousands of true statements of
    /// which a few dozen are worth acting on, so the main report carries the high and medium ones with a
    /// short list at the very top, and everything low confidence goes to a separate verbose file.
    /// </summary>
    public static class FindingReportWriter
    {
        private const string Caveats = "Everything below was found by reading compiled metadata. Reflection, "
            + "SendMessage, Invoke by name, UnityEvents wired in the inspector, animation events and "
            + "references coming from scenes, prefabs or other assets are all invisible to this scan. "
            + "Treat every entry as a candidate to verify, never as proof.";

        private const string CodeFence = "```";
        private const string CommentPrefix = "# ";
        private const string EmptySection = "Nothing found.";
        private const string IgnoreMarker = "graph-ignore";
        private const string MainTitle = "# Codebase Graph findings";
        private const string NothingDismissed = "Nothing is dismissed right now.";
        private const int StartHereCount = 20;
        private const string VerboseTitle = "# Codebase Graph findings, low confidence";

        /// <summary>Order the sections appear in.</summary>
        private static readonly EFinding[] SectionOrder =
        {
            EFinding.DeadType,
            EFinding.DeadMember,
            EFinding.UnimplementedInterfaceMember,
            EFinding.SerializedNeverRead,
            EFinding.WriteOnlyField,
            EFinding.PrivateCandidate,
            EFinding.PublicButInternalOnly,
            EFinding.ReadOnlyCandidate,
            EFinding.StaticMutableState,
            EFinding.TypeCycle,
            EFinding.NamespaceCycle,
            EFinding.GodClass,
            EFinding.HighInstability,
            EFinding.UnusedInterfaceMember,
            EFinding.UnusedPublicApi
        };

        /// <summary>Builds the main report, holding the findings worth reading first.</summary>
        /// <param name="graph">Graph to report on.</param>
        /// <returns>The Markdown text.</returns>
        public static string BuildMain(CodebaseGraphData graph)
        {
            List<FindingEntry> entries = Collect(graph);
            StringBuilder builder = new();

            AppendHeader(builder, graph, MainTitle, entries);
            AppendStartHere(builder, entries);
            AppendSections(builder, entries, ESeverity.Medium);
            AppendIgnoreMarker(builder);
            AppendDismissals(builder);

            return builder.ToString();
        }

        /// <summary>Builds the companion report holding everything ranked low.</summary>
        /// <param name="graph">Graph to report on.</param>
        /// <returns>The Markdown text.</returns>
        public static string BuildVerbose(CodebaseGraphData graph)
        {
            List<FindingEntry> entries = Collect(graph);
            StringBuilder builder = new();

            builder.AppendLine(VerboseTitle);
            builder.AppendLine();
            builder.AppendLine("Everything here is true and almost none of it is worth acting on. Public "
                + "API of a distributable package, interface contracts, enum members and serialized "
                + "fields all end up in this file. Read it when you are deliberately shrinking something.");

            builder.AppendLine();
            AppendSections(builder, entries, ESeverity.Low);

            return builder.ToString();
        }

        private static List<FindingEntry> Collect(CodebaseGraphData graph)
        {
            List<FindingEntry> entries = new();
            Dictionary<string, string[]> sources = new(StringComparer.Ordinal);
            HashSet<string> reportedCycles = new(StringComparer.Ordinal);

            foreach (NamespaceNodeInfo group in graph.Namespaces.Values)
                CollectNamespace(group, entries, reportedCycles);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                CollectType(type, entries, sources, reportedCycles);
                CollectMembers(type, entries, sources);
            }

            entries.Sort(Compare);
            return entries;
        }

        private static int Compare(FindingEntry left, FindingEntry right)
        {
            int bySeverity = left.Severity.CompareTo(right.Severity);
            if (bySeverity != 0)
                return bySeverity;

            int byFinding = left.Finding.CompareTo(right.Finding);

            return byFinding != 0
                ? byFinding
                : string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
        }

        private static void CollectNamespace(NamespaceNodeInfo group,
            List<FindingEntry> entries,
            HashSet<string> reportedCycles)
        {
            if (group.CyclePartners.Count == 0 || FindingCatalog.IsHidden(group))
                return;

            if (!reportedCycles.Add(group.CycleId))
                return;

            entries.Add(new FindingEntry(EFinding.NamespaceCycle,
                ESeverity.Medium,
                group.DismissalId,
                string.Empty,
                BuildCycleDetail(group.Name, group.CyclePartners)));
        }

        private static void CollectType(TypeNodeInfo type,
            List<FindingEntry> entries,
            Dictionary<string, string[]> sources,
            HashSet<string> reportedCycles)
        {
            if (type.Issues == ETypeIssue.None || FindingCatalog.IsHidden(type))
                return;

            List<EFinding> findings = new();
            FindingCatalog.Collect(type, findings);

            string location = BuildLocation(type, null, sources);

            foreach (EFinding finding in findings)
            {
                if (finding == EFinding.TypeCycle && !reportedCycles.Add(type.CycleId))
                    continue;

                string detail = finding == EFinding.TypeCycle
                    ? BuildCycleDetail(type.ShortName, type.CyclePartners)
                    : string.Empty;

                entries.Add(new FindingEntry(finding,
                    FindingSeverity.Resolve(finding, type),
                    type.DismissalId,
                    location,
                    detail));
            }
        }

        private static string BuildCycleDetail(string owner, List<string> partners)
        {
            List<string> members = new(partners) { owner };
            members.Sort(StringComparer.Ordinal);

            return $" the whole loop is {members.Count} deep: {string.Join(", ", members)}";
        }

        private static string BuildAssetDetail(EFinding finding, MemberNodeInfo member)
        {
            if (finding != EFinding.SerializedNeverRead)
                return string.Empty;

            return member.AssetUsageCount == 0
                ? " no prefab, scene or asset sets it either"
                : $" set on {member.AssetUsageCount} prefabs, scenes or assets";
        }

        private static void CollectMembers(TypeNodeInfo type,
            List<FindingEntry> entries,
            Dictionary<string, string[]> sources)
        {
            foreach (MemberNodeInfo member in type.Members)
            {
                if (member.Issues == EMemberIssue.None)
                    continue;

                List<EFinding> findings = new();
                FindingCatalog.Collect(member, type, findings);

                string location = BuildLocation(type, member, sources);

                foreach (EFinding finding in findings)
                {
                    entries.Add(new FindingEntry(finding,
                        FindingSeverity.Resolve(finding, member, type),
                        member.DismissalId,
                        location,
                        BuildAssetDetail(finding, member)));
                }
            }
        }

        private static void AppendHeader(StringBuilder builder,
            CodebaseGraphData graph,
            string title,
            List<FindingEntry> entries)
        {
            int packages = graph.PackageAssemblies.Count;
            int project = graph.ScannedAssemblies.Count - packages;

            builder.AppendLine(title);
            builder.AppendLine();
            builder.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd HH:mm}.");
            builder.AppendLine($"Scanned {graph.TypeCount} types and {graph.MemberCount} members in "
                + $"{graph.ScanSeconds:0.0} seconds, across {packages} package assemblies and {project} "
                + "from the project itself.");

            builder.AppendLine($"{graph.CountExcludedTypes()} types were left out as generated, sample or "
                + "test code. Public API findings on package assemblies are ranked low and moved to the "
                + "companion file.");

            builder.AppendLine();
            builder.AppendLine($"{Count(entries, ESeverity.High)} findings ranked high, "
                + $"{Count(entries, ESeverity.Medium)} medium, {Count(entries, ESeverity.Low)} low.");

            builder.AppendLine();
            builder.AppendLine("## How to read this");
            builder.AppendLine();
            builder.AppendLine(Caveats);
            builder.AppendLine();
            builder.AppendLine("Every entry is written as its dismissal id, so it can be copied straight "
                + "into the instruction block at the end. The three shapes are "
                + "`namespace:<full namespace>`, `type:<full type name>` and "
                + "`member:<full type name>#<member signature>`.");

            builder.AppendLine();
        }

        private static int Count(List<FindingEntry> entries, ESeverity severity)
        {
            int count = 0;

            foreach (FindingEntry entry in entries)
            {
                if (entry.Severity == severity)
                    count++;
            }

            return count;
        }

        private static void AppendStartHere(StringBuilder builder, List<FindingEntry> entries)
        {
            builder.AppendLine("## Start here");
            builder.AppendLine();

            int written = 0;

            foreach (FindingEntry entry in entries)
            {
                if (entry.Severity != ESeverity.High || written >= StartHereCount)
                    continue;

                builder.AppendLine($"{entry.Format()}  ({FindingCatalog.Describe(entry.Finding).Title})");
                written++;
            }

            if (written == 0)
                builder.AppendLine("Nothing ranked high. That is a good sign.");

            builder.AppendLine();
        }

        private static void AppendSections(StringBuilder builder,
            List<FindingEntry> entries,
            ESeverity worstIncluded)
        {
            foreach (EFinding finding in SectionOrder)
                AppendSection(builder, entries, finding, worstIncluded);
        }

        private static void AppendSection(StringBuilder builder,
            List<FindingEntry> entries,
            EFinding finding,
            ESeverity worstIncluded)
        {
            List<FindingEntry> matching = new();

            foreach (FindingEntry entry in entries)
            {
                bool wanted = worstIncluded == ESeverity.Low
                    ? entry.Severity == ESeverity.Low
                    : entry.Severity <= worstIncluded;

                if (entry.Finding == finding && wanted)
                    matching.Add(entry);
            }

            if (matching.Count == 0)
                return;

            FindingDescriptor descriptor = FindingCatalog.Describe(finding);

            builder.AppendLine($"## {descriptor.Title} ({matching.Count})");
            builder.AppendLine();
            builder.AppendLine($"**What the scan saw.** {descriptor.Explanation}");
            builder.AppendLine();
            builder.AppendLine($"**What to do.** {descriptor.Action}");
            builder.AppendLine();

            foreach (FindingEntry entry in matching)
                builder.AppendLine(entry.Format());

            builder.AppendLine();
        }

        private static void AppendIgnoreMarker(StringBuilder builder)
        {
            builder.AppendLine("## Silencing a finding in the source");
            builder.AppendLine();
            builder.AppendLine($"Putting `{IgnoreMarker}` in a comment on the same line as a member "
                + "silences every finding on it, permanently and visibly, without touching the dismissal "
                + "file. A `[CodebaseGraphIgnore]` or `[TroubleshootSample]` attribute does the same for a "
                + "whole type. Either is the better choice when the finding is wrong for a reason a "
                + "reader of the code should know about.");

            builder.AppendLine();
            builder.AppendLine("```csharp");
            builder.AppendLine($"private static int _instanceCount; // {IgnoreMarker}: cleared by the "
                + "bootstrapper, which the scan cannot see");

            builder.AppendLine("```");
            builder.AppendLine();
        }

        private static void AppendDismissals(StringBuilder builder)
        {
            builder.AppendLine("## Dismissed entries");
            builder.AppendLine();
            builder.AppendLine("These were reviewed and set aside, so none of them appear in the sections "
                + "above. To change that, edit the block below and hand it back through **Update "
                + "dismissals** in the window toolbar, either from the clipboard or as a file.");

            builder.AppendLine();
            builder.AppendLine($"- `{DismissalTextFormat.DismissVerb} <id>` hides the findings on that entry.");
            builder.AppendLine($"- `{DismissalTextFormat.DismissWithContentsVerb} <id>` also hides everything "
                + "inside a type or a namespace.");

            builder.AppendLine($"- `{DismissalTextFormat.RestoreVerb} <id>` brings a dismissed entry back.");
            builder.AppendLine($"- `{DismissalTextFormat.RestoreWithContentsVerb} <id>` brings it back "
                + "together with everything dismissed inside it.");
            builder.AppendLine("- Lines starting with a hash are comments, and blank lines are ignored.");
            builder.AppendLine();
            builder.AppendLine("The lines are instructions, not a replacement of the stored file, so "
                + "anything you leave out stays exactly as it is. Only `restore` removes a dismissal. "
                + "The window keeps the state in `ProjectSettings/CodebaseGraphDismissed.json`, which you "
                + "may also edit directly.");

            builder.AppendLine();
            builder.AppendLine(CodeFence);

            string current = DismissalTextFormat.Write();
            builder.AppendLine(string.IsNullOrEmpty(current)
                ? $"{CommentPrefix}{NothingDismissed}"
                : current.TrimEnd());

            builder.AppendLine(CodeFence);
            builder.AppendLine();
        }

        private static string BuildLocation(TypeNodeInfo type,
            MemberNodeInfo member,
            Dictionary<string, string[]> sources)
        {
            if (string.IsNullOrEmpty(type.ScriptPath))
                return string.Empty;

            if (!sources.TryGetValue(type.ScriptPath, out string[] lines))
            {
                lines = SourceLineLocator.Split(type.ScriptPath);
                sources[type.ScriptPath] = lines;
            }

            int line = SourceLineLocator.Find(lines, member, type.ShortName, type.Kind == ETypeKind.Interface);

            return $" - {type.ScriptPath}:{line}";
        }
    }
}
