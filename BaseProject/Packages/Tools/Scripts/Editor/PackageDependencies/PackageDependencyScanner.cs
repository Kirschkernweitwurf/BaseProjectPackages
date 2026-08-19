using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ToolPackage.Editor.PackageDependencies
{
    /// <summary>
    /// Derives the package dependency graph from the assembly definitions on disk, so the list the
    /// installer ships can be generated from the single source of truth instead of maintained by hand.
    /// <para>
    /// Reads every folder under the packages root that holds a <c>package.json</c>, maps each
    /// assembly to its owning package, resolves the references between them, and removes the edges
    /// another edge already implies so a package lists only what it needs directly.
    /// </para>
    /// </summary>
    internal static class PackageDependencyScanner
    {
        private const string AsmdefSearchPattern = "*.asmdef";
        private const string GuidPrefix = "GUID:";
        private const string ManifestFileName = "package.json";
        private const string MetaExtension = ".meta";
        private const string TestAssemblySuffix = ".Tests";

        private static readonly Regex GuidPattern = new(@"guid:\s*(\w+)", RegexOptions.Compiled);

        /// <summary>
        /// Scans the given packages root and returns one entry per package, sorted by display name.
        /// </summary>
        /// <param name="packagesRoot">The absolute path of the folder holding the package folders.</param>
        /// <returns>The scanned packages, or an empty array when the root holds none.</returns>
        internal static PackageDependencyInfo[] Scan(string packagesRoot)
        {
            if (!Directory.Exists(packagesRoot))
            {
                CustomLogger.LogWarning($"Packages root not found: {packagesRoot}", null);

                return Array.Empty<PackageDependencyInfo>();
            }

            List<string> folders = new();

            foreach (string folder in Directory.GetDirectories(packagesRoot))
            {
                if (File.Exists(Path.Combine(folder, ManifestFileName)))
                    folders.Add(folder);
            }

            if (folders.Count == 0)
            {
                CustomLogger.LogWarning($"No package folders found under {packagesRoot}.", null);

                return Array.Empty<PackageDependencyInfo>();
            }

            Dictionary<string, string> assemblyToPackage = new();
            Dictionary<string, string> guidToAssembly = new();
            List<KeyValuePair<string, AsmdefContent>> owned = new();

            foreach (string folder in folders)
                Collect(folder, assemblyToPackage, guidToAssembly, owned);

            Dictionary<string, HashSet<string>> edges = BuildEdges(owned, assemblyToPackage, guidToAssembly);
            List<PackageDependencyInfo> result = new();

            foreach (string folder in folders)
            {
                string package = Path.GetFileName(folder);
                edges.TryGetValue(package, out HashSet<string> direct);

                result.Add(new PackageDependencyInfo(package, PackageDisplayNames.Resolve(package),
                    Reduce(direct, edges)));
            }

            result.Sort(comparison: static (a, b)
                => string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal));

            return result.ToArray();
        }

        private static void Collect(string folder, IDictionary<string, string> assemblyToPackage,
            IDictionary<string, string> guidToAssembly, ICollection<KeyValuePair<string, AsmdefContent>> owned)
        {
            string package = Path.GetFileName(folder);

            foreach (string path in Directory.GetFiles(folder, AsmdefSearchPattern, SearchOption.AllDirectories))
            {
                AsmdefContent content = JsonUtility.FromJson<AsmdefContent>(File.ReadAllText(path));

                if (content == null || string.IsNullOrEmpty(content.Name))
                    continue;

                assemblyToPackage[content.Name] = package;

                string guid = ReadGuid(path + MetaExtension);

                if (!string.IsNullOrEmpty(guid))
                    guidToAssembly[guid] = content.Name;

                // An assembly behind a define constraint is optional by design and a test assembly
                // never ships, so neither may turn into a hard dependency of the owning package.
                if (content.IsOptional || content.Name.EndsWith(TestAssemblySuffix, StringComparison.Ordinal))
                    continue;

                owned.Add(new KeyValuePair<string, AsmdefContent>(package, content));
            }
        }

        private static Dictionary<string, HashSet<string>> BuildEdges(
            IEnumerable<KeyValuePair<string, AsmdefContent>> owned,
            IReadOnlyDictionary<string, string> assemblyToPackage,
            IReadOnlyDictionary<string, string> guidToAssembly)
        {
            Dictionary<string, HashSet<string>> edges = new();

            foreach (KeyValuePair<string, AsmdefContent> pair in owned)
            {
                foreach (string reference in pair.Value.References)
                {
                    string assembly = Resolve(reference, guidToAssembly);

                    if (assembly == null || !assemblyToPackage.TryGetValue(assembly, out string target))
                        continue;

                    if (target == pair.Key)
                        continue;

                    if (!edges.TryGetValue(pair.Key, out HashSet<string> set))
                    {
                        set = new HashSet<string>();
                        edges[pair.Key] = set;
                    }

                    set.Add(target);
                }
            }

            return edges;
        }

        // An edge is redundant when another edge of the same package already reaches its target.
        private static string[] Reduce(HashSet<string> direct, IReadOnlyDictionary<string, HashSet<string>> edges)
        {
            if (direct == null)
                return Array.Empty<string>();

            List<string> kept = new();

            foreach (string candidate in direct)
            {
                if (!IsImplied(candidate, direct, edges))
                    kept.Add(candidate);
            }

            kept.Sort(StringComparer.Ordinal);

            return kept.ToArray();
        }

        private static bool IsImplied(string candidate, IEnumerable<string> direct,
            IReadOnlyDictionary<string, HashSet<string>> edges)
        {
            foreach (string other in direct)
            {
                if (other == candidate)
                    continue;

                if (Reachable(other, candidate, edges, new HashSet<string>()))
                    return true;
            }

            return false;
        }

        private static bool Reachable(string from, string target,
            IReadOnlyDictionary<string, HashSet<string>> edges, ISet<string> visited)
        {
            if (!visited.Add(from) || !edges.TryGetValue(from, out HashSet<string> next))
                return false;

            if (next.Contains(target))
                return true;

            foreach (string step in next)
            {
                if (Reachable(step, target, edges, visited))
                    return true;
            }

            return false;
        }

        private static string Resolve(string reference, IReadOnlyDictionary<string, string> guidToAssembly)
        {
            if (string.IsNullOrEmpty(reference))
                return null;

            if (!reference.StartsWith(GuidPrefix, StringComparison.Ordinal))
                return reference;

            return guidToAssembly.GetValueOrDefault(reference[GuidPrefix.Length..]);
        }

        private static string ReadGuid(string metaPath)
        {
            if (!File.Exists(metaPath))
                return string.Empty;

            Match match = GuidPattern.Match(File.ReadAllText(metaPath));

            return match.Success
                ? match.Groups[1].Value
                : string.Empty;
        }
    }
}