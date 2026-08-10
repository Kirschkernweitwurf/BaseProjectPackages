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
    /// A scan of a real project produces thousands of true statements of which a few dozen are worth
    /// acting on, so everything is ranked and the twenty highest are listed at the very top. The low
    /// confidence findings still follow, after the parts you act on, under a heading that says plainly
    /// they are there for reference. Ranking is what makes the file readable, so splitting it in two
    /// bought nothing and cost the second half its header, its caveats and its dismissal block.
    /// </summary>
    internal static class FindingReportWriter
    {
        private const string Caveats = "Everything below was found by reading compiled metadata. Reflection, "
            + "SendMessage, Invoke by name, UnityEvents wired in the inspector, animation events and "
            + "references coming from scenes, prefabs or other assets are all invisible to this scan. "
            + "Treat every entry as a candidate to verify, never as proof.";

        private const string CodeFence = "```";
        private const string CommentPrefix = "# ";
        private const string IgnoreMarker = "graph-ignore";
        private const string NothingDismissed = "Nothing is dismissed right now.";
        private const string ReferenceTitle = "# For reference: low confidence findings";
        private const string ReportTitle = "# Codebase Graph findings";
        private const string SectionMarker = "##";
        private const int SizeProfileCount = 20;
        private const string SizeProfileTitle = "# Size profile, for tuning the very large type threshold";
        private const int StartHereCount = 20;
        private const string SubsectionMarker = "###";
        private const string UnknownReason = "Unstated";

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

        /// <summary>Builds the report.</summary>
        /// <param name="graph">Graph to report on.</param>
        /// <returns>The Markdown text.</returns>
        public static string Build(CodebaseGraphData graph)
        {
            List<FindingEntry> entries = Collect(graph);
            StringBuilder builder = new();

            AppendHeader(builder, graph, entries);
            AppendStartHere(builder, entries);
            AppendSections(builder, entries, false);
            AppendIgnoreMarker(builder, graph);
            AppendDismissals(builder, graph);
            AppendReference(builder, entries);
            AppendSizeProfile(builder, graph);

            return builder.ToString();
        }

        /// <summary>
        /// Appends the low confidence findings after everything actionable. They sit at the end rather
        /// than in a file of their own, because they are true and worth having on hand, just never the
        /// reason anyone opened the report.
        /// </summary>
        private static void AppendReference(StringBuilder builder, List<FindingEntry> entries)
        {
            if (Count(entries, ESeverity.Low) == 0)
                return;

            builder.AppendLine(ReferenceTitle);
            builder.AppendLine();
            builder.AppendLine("Everything past this point is true and almost none of it is worth acting "
                + "on. The published API of a distributable package, interface contracts, enum members "
                + "and serialized fields all land here. Read it when you are deliberately shrinking "
                + "something, and otherwise stop at the section above.");

            builder.AppendLine();
            AppendSections(builder, entries, true);
        }

        /// <summary>
        /// Lists the largest types by compiled size. The size threshold behind the very large type finding cannot
        /// be picked sensibly from a guess, because what counts as big depends entirely on the codebase.
        /// So the report shows the top of the distribution and the number can be set from that.
        /// </summary>
        private static void AppendSizeProfile(StringBuilder builder, CodebaseGraphData graph)
        {
            List<TypeNodeInfo> largest = new(graph.Types.Values);
            largest.Sort((left, right) => right.IlSize.CompareTo(left.IlSize));

            builder.AppendLine(SizeProfileTitle);
            builder.AppendLine();
            builder.AppendLine("Compiled size folds in lambdas, local functions, iterators and async "
                + "bodies, so a coroutine heavy type accumulates bytes without being structurally large. "
                + "Read this alongside the namespace reach beside it, which measures something the "
                + "compiler cannot inflate.");

            builder.AppendLine();

            int shown = largest.Count < SizeProfileCount
                ? largest.Count
                : SizeProfileCount;

            for (int index = 0; index < shown; index++)
            {
                TypeNodeInfo type = largest[index];

                builder.AppendLine($"- `{type.FullName}` - {type.IlSize} bytes, {type.Members.Count} "
                    + $"members, reaches {type.NamespaceReach} namespaces");
            }

            builder.AppendLine();
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
                GraphIdentity.ForFinding(group.DismissalId, EFinding.NamespaceCycle),
                string.Empty,
                BuildCycleDetail(group.CycleDescription, group.CycleCutHint, group.CycleComponentSize)));
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
                    ? BuildCycleDetail(type.CycleDescription, type.CycleCutHint, type.CycleComponentSize)
                    : string.Empty;

                entries.Add(new FindingEntry(finding,
                    FindingSeverity.Resolve(finding, type),
                    GraphIdentity.ForFinding(type.DismissalId, finding),
                    location,
                    detail));
            }
        }

        private static string BuildCycleDetail(string description, string cut, int componentSize)
        {
            string tangle = componentSize > 2
                ? $" It sits inside a tangle of {componentSize}."
                : string.Empty;

            string hint = string.IsNullOrEmpty(cut)
                ? string.Empty
                : $" Cheapest edge to cut: {cut}.";

            return $" Loop: {description}.{hint}{tangle}";
        }

        private static string BuildAssetDetail(EFinding finding, MemberNodeInfo member)
        {
            if (finding != EFinding.SerializedNeverRead)
                return string.Empty;

            if (member.AssetUsageCount == 0)
                return " no prefab, scene or asset sets it either";

            return member.AssetUsageCount == 1
                ? " set on 1 prefab, scene or asset"
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
                        GraphIdentity.ForFinding(member.DismissalId, finding),
                        location,
                        BuildAssetDetail(finding, member)));
                }
            }
        }

        private static void AppendHeader(StringBuilder builder,
            CodebaseGraphData graph,
            List<FindingEntry> entries)
        {
            int packages = graph.PackageAssemblies.Count;
            int project = graph.ScannedAssemblies.Count - packages;

            builder.AppendLine(ReportTitle);
            builder.AppendLine();
            builder.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd HH:mm}.");
            builder.AppendLine($"Scanned {graph.TypeCount} types and {graph.MemberCount} members in "
                + $"{graph.ScanSeconds:0.0} seconds, across {packages} package assemblies and {project} "
                + "from the project itself.");

            builder.AppendLine($"{graph.CountExcludedTypes()} types were left out as generated, sample or "
                + "test code. Published package API is ranked low and moved to the companion file.");

            builder.AppendLine();
            AppendExclusions(builder, graph);

            builder.AppendLine();
            builder.AppendLine($"{Count(entries, ESeverity.High)} findings ranked high, "
                + $"{Count(entries, ESeverity.Medium)} medium, {Count(entries, ESeverity.Low)} low. The "
                + "low ones are gathered at the end, after everything worth acting on.");

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

        /// <summary>
        /// Lists why types were left out. An exclusion that quietly failed to happen is otherwise
        /// invisible, and the only symptom is generated code showing up in the findings.
        /// </summary>
        private static void AppendExclusions(StringBuilder builder, CodebaseGraphData graph)
        {
            Dictionary<string, int> byReason = new(StringComparer.Ordinal);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (!type.IsExcludedFromFindings)
                    continue;

                string reason = type.ExclusionReason ?? UnknownReason;
                byReason.TryGetValue(reason, out int count);
                byReason[reason] = count + 1;
            }

            if (byReason.Count == 0)
                return;

            List<string> reasons = new(byReason.Keys);
            reasons.Sort(StringComparer.Ordinal);

            builder.AppendLine("Left out:");

            foreach (string reason in reasons)
                builder.AppendLine($"- {reason}: {byReason[reason]} types");

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

        private static void AppendSections(StringBuilder builder, List<FindingEntry> entries, bool wantsLow)
        {
            foreach (EFinding finding in SectionOrder)
                AppendSection(builder, entries, finding, wantsLow);
        }

        private static void AppendSection(StringBuilder builder,
            List<FindingEntry> entries,
            EFinding finding,
            bool wantsLow)
        {
            List<FindingEntry> matching = new();

            foreach (FindingEntry entry in entries)
            {
                bool isLow = entry.Severity == ESeverity.Low;

                if (entry.Finding == finding && isLow == wantsLow)
                    matching.Add(entry);
            }

            if (matching.Count == 0)
                return;

            FindingDescriptor descriptor = FindingCatalog.Describe(finding);
            string heading = wantsLow
                ? SubsectionMarker
                : SectionMarker;

            builder.AppendLine($"{heading} {descriptor.Title} ({matching.Count})");
            builder.AppendLine();
            builder.AppendLine($"**What the scan saw.** {descriptor.Explanation}");
            builder.AppendLine();
            builder.AppendLine($"**What to do.** {descriptor.Action}");
            builder.AppendLine();

            foreach (FindingEntry entry in matching)
                builder.AppendLine(entry.Format());

            builder.AppendLine();
        }

        private static void AppendIgnoreMarker(StringBuilder builder, CodebaseGraphData graph)
        {
            builder.AppendLine("## Silencing a finding");
            builder.AppendLine();
            builder.AppendLine("Why a finding is wrong belongs at the declaration as an ordinary comment, "
                + "because it is worth writing whether or not this tool exists. Silencing it is tool "
                + "state and belongs in the dismissal block below. Doing both keeps the reasoning next to "
                + "the code and keeps the decision reviewable.");

            builder.AppendLine();
            builder.AppendLine("An id names one finding on one entry, and the ids below are already "
                + "written that way. Dismissing the whole entry instead is possible, by dropping the "
                + "part after the bar, but it silences findings nobody has looked at, including ones a "
                + "later scan raises for the first time.");

            builder.AppendLine();
            builder.AppendLine("An id also embeds the signature it was written for, so renaming or "
                + "retyping the member brings the finding back and the stale entry is listed for review. "
                + "That is deliberate: a silencing mechanism that survives arbitrary refactoring is one "
                + "that can outlive its reason without telling anyone. A dismissal for a finding you "
                + "have since fixed goes stale the same way, and is listed the same way.");

            builder.AppendLine();
            builder.AppendLine("For a whole type that should never be reported at all, a fixture or a "
                + "generator's output, put `[CodebaseGraphIgnore]` on the type instead. That is a property "
                + "of the type rather than of anyone's review state, it survives renaming on purpose, and "
                + "one attribute replaces every id the type would otherwise need.");

            builder.AppendLine();
            builder.AppendLine($"A same line `{IgnoreMarker}` comment is still honored for compatibility, "
                + "but it is no longer the recommended route: it silences by name, so it rides through "
                + "renames and keeps working after the member has stopped meaning what it meant.");

            builder.AppendLine();
            AppendUnmatchedMarkers(builder, graph);
        }

        private static void AppendUnmatchedMarkers(StringBuilder builder, CodebaseGraphData graph)
        {
            if (graph.UnmatchedIgnoreMarkers.Count == 0)
                return;

            builder.AppendLine($"### Markers that silenced nothing ({graph.UnmatchedIgnoreMarkers.Count})");
            builder.AppendLine();
            builder.AppendLine($"These lines carry `{IgnoreMarker}` but no member is declared on them, so "
                + "they had no effect. The marker only reads the code in front of the comment, which is "
                + "why one written on the line above a field does nothing.");

            builder.AppendLine();

            foreach (string location in graph.UnmatchedIgnoreMarkers)
                builder.AppendLine($"- {location}");

            builder.AppendLine();
        }

        private static void AppendDismissals(StringBuilder builder, CodebaseGraphData graph)
        {
            List<DismissalEntry> stored = DismissalStore.Collect();
            DismissalAudit.Apply(graph, stored);

            int missing = DismissalAudit.Count(stored, EStaleReason.Missing);
            int resolved = DismissalAudit.Count(stored, EStaleReason.Resolved);
            int stale = missing + resolved;

            builder.AppendLine("## Dismissed entries");
            builder.AppendLine();
            builder.AppendLine($"{stored.Count - stale} dismissals active. {missing} point at something "
                + $"that no longer exists, and {resolved} silence a finding that is no longer raised, "
                + "which usually means it was fixed. Both are listed in the Dismissed window in the "
                + "graph toolbar and neither is removed for you.");

            builder.AppendLine();
            builder.AppendLine("These were reviewed and dismissed, so none of them appear anywhere in "
                + "this report. To change that, edit the block below and hand it back through **Update "
                + "dismissals** in the window toolbar, either from the clipboard or as a file. That "
                + "button only reads; this report is written by **Export findings** next to it.");

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