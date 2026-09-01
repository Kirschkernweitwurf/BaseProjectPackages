using System;
using System.Collections.Generic;
using System.Text;

namespace Base.ToolPackage.Editor.AssemblyGraph.Architecture
{
    /// <summary>
    /// Writes the rolled up assembly edges out as text. Nothing in the architecture analysis is worth
    /// trusting until these numbers have been read once against a codebase somebody already knows, so
    /// this exists before any rule does, and stays afterward as the thing to open when a finding looks
    /// wrong.
    /// <br/><br/>
    /// The two cross checks at the end are the ones that catch a roll-up losing edges rather than a
    /// project having problems. A declared reference with no edge is either dead weight in the asmdef
    /// or a usage the scan failed to see. An edge with no declared reference cannot compile, so if one
    /// ever appears the roll-up is attributing types to the wrong assembly.
    /// </summary>
    internal static class AssemblyEdgeReportWriter
    {
        private const string DeclaredHeading = "## Declared references with no rolled up edge";

        private const string DeclaredNote = "Either the reference is unused, or the scan did not see a "
            + "usage that is really there. Check one by hand before believing the column.";

        private const string EdgeHeading = "## Edges, narrowest first";

        private const string EdgeNote = "Weight counts distinct target types, with nested types folded "
            + "into their outermost owner. Usages counts member level references and is only context.";

        private const string ExcludedMarker = " [generated, sample or test source only]";
        private const string NoneText = "None.";
        private const string SummaryHeading = "# Assembly edge roll-up";
        private const string TypeCountHeading = "## Types per assembly";
        private const string UndeclaredHeading = "## Rolled up edges with no declared reference";

        private const string UndeclaredNote = "This list must stay empty. Code cannot reference an "
            + "assembly the asmdef does not, so an entry here means the roll-up is wrong.";

        /// <summary>Builds the report.</summary>
        /// <param name="edges">The rolled up assembly graph.</param>
        /// <param name="nodes">The declared assembly references, used for the cross checks.</param>
        /// <returns>The Markdown text.</returns>
        internal static string Build(AssemblyEdgeGraph edges, IReadOnlyList<AssemblyNodeInfo> nodes)
        {
            StringBuilder builder = new();
            Dictionary<string, HashSet<string>> declared = CollectDeclared(nodes);

            builder.AppendLine(SummaryHeading);
            builder.AppendLine();
            builder.AppendLine($"{edges.Assemblies.Count} assemblies, {edges.Edges.Count} edges.");
            builder.AppendLine();

            AppendEdges(builder, edges);
            AppendDeclaredWithoutEdge(builder, edges, declared);
            AppendEdgeWithoutDeclaration(builder, edges, declared);
            AppendTypeCounts(builder, edges);

            return builder.ToString();
        }

        private static Dictionary<string, HashSet<string>> CollectDeclared(IReadOnlyList<AssemblyNodeInfo> nodes)
        {
            Dictionary<string, HashSet<string>> declared = new(StringComparer.Ordinal);

            if (nodes == null)
                return declared;

            foreach (AssemblyNodeInfo node in nodes)
            {
                HashSet<string> targets = new(StringComparer.Ordinal);

                foreach (AssemblyReferenceInfo reference in node.References)
                    targets.Add(reference.TargetName);

                declared[node.Name] = targets;
            }

            return declared;
        }

        private static void AppendEdges(StringBuilder builder, AssemblyEdgeGraph edges)
        {
            builder.AppendLine(EdgeHeading);
            builder.AppendLine();
            builder.AppendLine(EdgeNote);
            builder.AppendLine();

            List<AssemblyEdgeInfo> sorted = new(edges.Edges);

            sorted.Sort(comparison: static (left, right) =>
            {
                int byWeight = left.Weight.CompareTo(right.Weight);

                return byWeight != 0
                    ? byWeight
                    : string.Compare(left.Key.ToString(), right.Key.ToString(), StringComparison.Ordinal);
            });

            foreach (AssemblyEdgeInfo edge in sorted)
            {
                string excluded = edge.IsEntirelyExcluded
                    ? ExcludedMarker
                    : string.Empty;

                builder.AppendLine($"- **{edge.Weight}** types, {edge.UsageCount} usages: "
                    + $"`{edge.SourceName}` -> `{edge.TargetName}`{excluded}");

                foreach (string typeName in edge.TargetTypeNames)
                    builder.AppendLine($"  - {typeName}");
            }

            builder.AppendLine();
        }

        private static void AppendDeclaredWithoutEdge(StringBuilder builder,
            AssemblyEdgeGraph edges,
            Dictionary<string, HashSet<string>> declared)
        {
            builder.AppendLine(DeclaredHeading);
            builder.AppendLine();
            builder.AppendLine(DeclaredNote);
            builder.AppendLine();

            int written = 0;

            foreach (string source in edges.Assemblies)
            {
                if (!declared.TryGetValue(source, out HashSet<string> targets))
                    continue;

                foreach (string target in targets)
                {
                    // Only assemblies that were scanned can produce an edge, so an unscanned target
                    // says nothing about whether the reference is needed.
                    if (edges.CountTypes(target) == 0)
                        continue;

                    if (edges.Find(source, target) != null)
                        continue;

                    builder.AppendLine($"- `{source}` -> `{target}`");
                    written++;
                }
            }

            AppendNoneWhenEmpty(builder, written);
        }

        private static void AppendEdgeWithoutDeclaration(StringBuilder builder,
            AssemblyEdgeGraph edges,
            Dictionary<string, HashSet<string>> declared)
        {
            builder.AppendLine(UndeclaredHeading);
            builder.AppendLine();
            builder.AppendLine(UndeclaredNote);
            builder.AppendLine();

            int written = 0;

            foreach (AssemblyEdgeInfo edge in edges.Edges)
            {
                if (!declared.TryGetValue(edge.SourceName, out HashSet<string> targets))
                    continue;

                if (targets.Contains(edge.TargetName))
                    continue;

                builder.AppendLine($"- `{edge.SourceName}` -> `{edge.TargetName}` "
                    + $"({edge.Weight} types)");

                written++;
            }

            AppendNoneWhenEmpty(builder, written);
        }

        private static void AppendTypeCounts(StringBuilder builder, AssemblyEdgeGraph edges)
        {
            builder.AppendLine(TypeCountHeading);
            builder.AppendLine();

            foreach (string assembly in edges.Assemblies)
                builder.AppendLine($"- `{assembly}`: {edges.CountTypes(assembly)}");

            builder.AppendLine();
        }

        private static void AppendNoneWhenEmpty(StringBuilder builder, int written)
        {
            if (written == 0)
                builder.AppendLine(NoneText);

            builder.AppendLine();
        }
    }
}