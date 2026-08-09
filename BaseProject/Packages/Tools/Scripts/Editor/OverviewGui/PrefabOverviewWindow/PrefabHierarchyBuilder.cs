using System.Collections.Generic;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// Links scanned prefabs into variant trees and flags the entries that look problematic.
    /// </summary>
    public static class PrefabHierarchyBuilder
    {
        private const int DeepChainDepth = 3;
        private const int HeavyOverrideCount = 30;

        /// <summary>
        /// Connects every variant to its base, fills in depth and variant counts, and flags issues.
        /// </summary>
        /// <param name="entries">All entries returned by the scanner.</param>
        /// <param name="overridesAnalyzed">True when the scan counted overrides, which enables more checks.</param>
        /// <returns>The prefabs that start a variant chain, sorted by name.</returns>
        public static List<PrefabEntry> Build(List<PrefabEntry> entries, bool overridesAnalyzed)
        {
            Dictionary<string, PrefabEntry> byGuid = new();

            foreach (PrefabEntry entry in entries)
                byGuid[entry.Guid] = entry;

            List<PrefabEntry> roots = new();

            foreach (PrefabEntry entry in entries)
            {
                PrefabEntry baseEntry = ResolveBase(entry, byGuid);

                if (baseEntry == null)
                {
                    roots.Add(entry);
                    continue;
                }

                entry.BaseEntry = baseEntry;
                baseEntry.AddChild(entry);
            }

            roots.Sort(PrefabEntry.CompareByName);

            HashSet<string> visited = new();

            foreach (PrefabEntry root in roots)
            {
                Prepare(root, 0, overridesAnalyzed, visited);
                CountVariants(root);
            }

            return roots;
        }

        private static PrefabEntry ResolveBase(PrefabEntry entry, Dictionary<string, PrefabEntry> byGuid)
        {
            if (string.IsNullOrEmpty(entry.BaseGuid))
                return null;

            if (!byGuid.TryGetValue(entry.BaseGuid, out PrefabEntry baseEntry))
                return null;

            return baseEntry == entry
                ? null
                : baseEntry;
        }

        private static void Prepare(PrefabEntry entry, int depth, bool overridesAnalyzed, HashSet<string> visited)
        {
            if (!visited.Add(entry.Guid))
                return;

            entry.Depth = depth;
            entry.Issues = Evaluate(entry, depth, overridesAnalyzed);
            entry.SortChildren();

            foreach (PrefabEntry child in entry.Children)
                Prepare(child, depth + 1, overridesAnalyzed, visited);
        }

        private static int CountVariants(PrefabEntry entry)
        {
            int total = 0;

            foreach (PrefabEntry child in entry.Children)
                total += 1 + CountVariants(child);

            entry.TotalVariants = total;

            return total;
        }

        private static EPrefabIssue Evaluate(PrefabEntry entry, int depth, bool overridesAnalyzed)
        {
            EPrefabIssue issues = EPrefabIssue.None;

            if (entry.Kind == EPrefabKind.Broken)
                issues |= EPrefabIssue.MissingBase;

            if (entry.Kind != EPrefabKind.Variant)
                return issues;

            if (entry.BaseEntry == null)
                issues |= EPrefabIssue.MissingBase;

            if (depth >= DeepChainDepth)
                issues |= EPrefabIssue.DeepChain;

            if (!overridesAnalyzed)
                return issues;

            if (entry.Overrides.IsEmpty)
                issues |= EPrefabIssue.RedundantVariant;

            if (entry.Overrides.Total >= HeavyOverrideCount)
                issues |= EPrefabIssue.HeavyOverrides;

            return issues;
        }
    }
}