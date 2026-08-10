using System;
using System.Collections.Generic;
using System.Text;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Analysis
{
    /// <summary>
    /// Writes everything about one namespace or one assembly and nothing about the rest. The findings
    /// report answers what is wrong across a project; this answers what one slice of it is, which is
    /// the thing worth handing to somebody, or something, that is about to work on that slice alone.
    /// <br/><br/>
    /// The boundary comes first on purpose. What a slice depends on and what depends on it is the part
    /// a reader cannot recover from the code in front of them. Getting it wrong is how a change
    /// that looked local turns out not to be.
    /// </summary>
    internal static class ScopeReportWriter
    {
        private const int MaximumMembers = 60;
        private const string TitleFormat = "# {0}";
        private const string UnknownFileText = "an unknown file";

        /// <summary>Builds the report for one scope.</summary>
        /// <param name="graph">Graph to read from.</param>
        /// <param name="scope">Namespace or assembly name to write about.</param>
        /// <param name="isAssembly">True when the scope names an assembly rather than a namespace.</param>
        /// <returns>The Markdown text.</returns>
        public static string Build(CodebaseGraphData graph, string scope, bool isAssembly)
        {
            List<TypeNodeInfo> types = CollectTypes(graph, scope, isAssembly);

            StringBuilder builder = new();
            builder.AppendLine(string.Format(TitleFormat, scope));
            builder.AppendLine();

            builder.AppendLine($"{types.Count} types. Everything below is inside this scope. Nothing "
                + "outside it is described, except where it is named as a dependency.");

            builder.AppendLine();

            AppendBoundary(builder, graph, types, scope);
            AppendTypes(builder, types);
            AppendFindings(builder, types);

            return builder.ToString();
        }

        private static List<TypeNodeInfo> CollectTypes(CodebaseGraphData graph, string scope, bool isAssembly)
        {
            List<TypeNodeInfo> types = new();

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                bool isInScope = isAssembly
                    ? type.AssemblyName == scope
                    : IsUnder(type.Namespace, scope);

                if (isInScope)
                    types.Add(type);
            }

            types.Sort((left, right) => string.Compare(left.FullName,
                right.FullName,
                StringComparison.OrdinalIgnoreCase));

            return types;
        }

        private static bool IsUnder(string name, string prefix)
            => name == prefix || name.StartsWith($"{prefix}.", StringComparison.Ordinal);

        /// <summary>
        /// Names what crosses the edge of the scope in both directions. Outward is what this code needs
        /// to keep working; inward is who breaks when it changes.
        /// </summary>
        private static void AppendBoundary(StringBuilder builder,
            CodebaseGraphData graph,
            List<TypeNodeInfo> types,
            string scope)
        {
            HashSet<TypeKey> inside = new();

            foreach (TypeNodeInfo type in types)
                inside.Add(type.Key);

            SortedSet<string> outward = new(StringComparer.Ordinal);
            SortedSet<string> inward = new(StringComparer.Ordinal);

            foreach (TypeNodeInfo type in types)
            {
                CollectCrossing(graph, type.Outgoing.Keys, inside, outward);
                CollectCrossing(graph, type.Incoming.Keys, inside, inward);
            }

            AppendList(builder, "## Depends on", outward, $"{scope} depends on nothing outside itself.");
            AppendList(builder, "## Depended on by", inward, $"Nothing outside {scope} uses it.");
        }

        private static void CollectCrossing(CodebaseGraphData graph,
            IEnumerable<TypeKey> keys,
            HashSet<TypeKey> inside,
            SortedSet<string> outside)
        {
            foreach (TypeKey key in keys)
            {
                if (inside.Contains(key))
                    continue;

                TypeNodeInfo other = graph.FindType(key);
                if (other != null)
                    outside.Add(other.Namespace);
            }
        }

        private static void AppendList(StringBuilder builder,
            string title,
            SortedSet<string> names,
            string emptyText)
        {
            builder.AppendLine(title);
            builder.AppendLine();

            if (names.Count == 0)
            {
                builder.AppendLine(emptyText);
                builder.AppendLine();
                return;
            }

            foreach (string name in names)
                builder.AppendLine($"- `{name}`");

            builder.AppendLine();
        }

        /// <summary>
        /// Lists the shape of each type without its bodies. That is usually enough to reason about a
        /// change, and it fits in a context window where the source would not.
        /// </summary>
        private static void AppendTypes(StringBuilder builder, List<TypeNodeInfo> types)
        {
            builder.AppendLine("## Types");
            builder.AppendLine();

            foreach (TypeNodeInfo type in types)
            {
                builder.AppendLine($"### {type.FullName}");
                builder.AppendLine();
                string path = string.IsNullOrEmpty(type.ScriptPath)
                    ? UnknownFileText
                    : type.ScriptPath;

                builder.AppendLine($"{type.Access} {type.Kind}, used by {type.FanIn}, uses {type.FanOut}."
                    + $" Declared in `{path}`.");

                builder.AppendLine();
                AppendMembers(builder, type);
            }
        }

        private static void AppendMembers(StringBuilder builder, TypeNodeInfo type)
        {
            int shown = 0;

            foreach (MemberNodeInfo member in type.Members)
            {
                if (shown == MaximumMembers)
                {
                    builder.AppendLine($"- and {type.Members.Count - MaximumMembers} more members");
                    break;
                }

                builder.AppendLine($"- `{member.Access} {member.Signature}`");
                shown++;
            }

            builder.AppendLine();
        }

        private static void AppendFindings(StringBuilder builder, List<TypeNodeInfo> types)
        {
            builder.AppendLine("## Findings in this scope");
            builder.AppendLine();

            int written = 0;

            foreach (TypeNodeInfo type in types)
                written += AppendTypeFindings(builder, type);

            if (written == 0)
                builder.AppendLine("Nothing was reported here.");

            builder.AppendLine();
        }

        private static int AppendTypeFindings(StringBuilder builder, TypeNodeInfo type)
        {
            int written = 0;
            List<EFinding> findings = new();

            FindingCatalog.Collect(type, findings);

            foreach (EFinding finding in findings)
            {
                builder.AppendLine($"- `{type.FullName}` - {FindingCatalog.Describe(finding).Title}");
                written++;
            }

            foreach (MemberNodeInfo member in type.Members)
            {
                findings.Clear();
                FindingCatalog.Collect(member, type, findings);

                foreach (EFinding finding in findings)
                {
                    builder.AppendLine($"- `{type.ShortName}.{member.Name}` - "
                        + FindingCatalog.Describe(finding).Title);

                    written++;
                }
            }

            return written;
        }
    }
}