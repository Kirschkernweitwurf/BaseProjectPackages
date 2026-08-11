using System;
using System.Collections.Generic;
using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.ToolPackage.Editor.NamingConventions.Scanning;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Scan results of the asset naming window and everything that narrows them down: the search
    /// text, the rule filter and the sort mode. Owns the raw scan, the filtered list, the groups
    /// and the dismissed paths, and keeps all four in step so the window only ever reads them.
    /// </summary>
    [Serializable]
    internal sealed class AssetNamingQuery
    {
        private const string RootFolder = "Assets";

        [SerializeField] private EAssetNamingSort sort = EAssetNamingSort.Folder;

        /// <summary>Sort and grouping mode of the result list.</summary>
        public EAssetNamingSort Sort
        {
            get => sort;
            set => sort = value;
        }

        /// <summary>Free text an asset path has to contain to stay in the list.</summary>
        public string Search
        {
            get => _search;
            set => _search = value;
        }

        /// <summary>Label of the only rule to show, or empty for all of them.</summary>
        public string RuleFilter
        {
            get => _ruleFilter;
            set => _ruleFilter = value;
        }

        /// <summary>Violations left after the filters, sorted by the current mode.</summary>
        public IReadOnlyList<AssetNamingViolation> Filtered => _filtered;

        /// <summary>Filtered violations split into the collapsible groups the sort mode asks for.</summary>
        public IReadOnlyList<AssetNamingGroup> Groups => _groups;

        /// <summary>Violations the last scan found, before any filter was applied.</summary>
        public int ScannedCount => _all.Count;

        /// <summary>Whether the project was scanned at least once since the window was opened.</summary>
        public bool HasScanned { get; private set; }

        /// <summary>Whether a search or a rule filter is hiding anything.</summary>
        public bool IsFilterActive => !string.IsNullOrWhiteSpace(_search) || _ruleFilter.Length > 0;

        /// <summary>Dismissed assets that still exist, counted before the search is applied.</summary>
        public int DismissedCount
        {
            get
            {
                _dismissedPaths ??= BuildDismissedPaths();

                return _dismissedPaths.Count;
            }
        }

        private readonly List<AssetNamingGroup> _groups = new();
        private readonly List<AssetNamingViolation> _all = new();
        private readonly List<AssetNamingViolation> _filtered = new();

        private List<string> _dismissedPaths;
        private string _ruleFilter = string.Empty;
        private string _search = string.Empty;

        /// <summary>Scans the project from scratch and reruns the filters.</summary>
        /// <param name="ruleSet">Rules the assets are checked against.</param>
        public void Scan(AssetNamingRuleSet ruleSet)
        {
            _all.Clear();
            _all.AddRange(AssetNamingScanner.Scan(ruleSet));
            HasScanned = true;

            // The dismissed rows keep resolved paths, and a rename leaves those stale even though
            // the GUID behind them is still right, so the cache is dropped with every scan.
            _dismissedPaths = null;
            Run();
        }

        /// <summary>Reapplies the filters and the sort to the last scan, then rebuilds the groups.</summary>
        public void Run()
        {
            _filtered.Clear();

            foreach (AssetNamingViolation violation in _all)
            {
                if (AssetNamingDismissStore.IsDismissed(violation.Guid))
                    continue;

                if (!IsMatchingFilter(violation))
                    continue;

                _filtered.Add(violation);
            }

            _filtered.Sort(Compare);
            RebuildGroups();
        }

        /// <summary>Splits the filtered list into the collapsible groups the sort mode asks for.</summary>
        public void RebuildGroups()
        {
            _groups.Clear();

            Dictionary<string, AssetNamingGroup> byKey = new();

            foreach (AssetNamingViolation violation in _filtered)
            {
                string key = GroupKeyOf(violation);

                if (!byKey.TryGetValue(key, out AssetNamingGroup group))
                {
                    group = new AssetNamingGroup(key);
                    byKey[key] = group;
                    _groups.Add(group);
                }

                group.Violations.Add(violation);
            }
        }

        /// <summary>
        /// Drops a violation that was just renamed away. The rows are drawn from the groups, so
        /// dropping it from the flat lists alone would leave a dead row behind that still offers
        /// Rename and Dismiss.
        /// </summary>
        /// <param name="violation">The violation that no longer applies.</param>
        public void Remove(AssetNamingViolation violation)
        {
            if (violation == null)
                return;

            _all.Remove(violation);
            _filtered.Remove(violation);
            RebuildGroups();
        }

        /// <summary>Forgets the resolved dismissed paths, so the next read looks them up again.</summary>
        public void InvalidateDismissed() => _dismissedPaths = null;

        /// <summary>Dismissed asset paths left after the search, in the order they are drawn.</summary>
        /// <returns>The cached list itself while no search is active, a filtered copy otherwise.</returns>
        public IReadOnlyList<string> GetVisibleDismissed()
        {
            _dismissedPaths ??= BuildDismissedPaths();

            if (string.IsNullOrWhiteSpace(_search))
                return _dismissedPaths;

            List<string> visible = new();

            foreach (string path in _dismissedPaths)
            {
                if (!path.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    continue;

                visible.Add(path);
            }

            return visible;
        }

        private static List<string> BuildDismissedPaths()
        {
            List<string> paths = new();

            foreach (string guid in AssetNamingDismissStore.GetAll())
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                    continue;

                paths.Add(path);
            }

            paths.Sort(StringComparer.Ordinal);

            return paths;
        }

        private bool IsMatchingFilter(AssetNamingViolation violation)
        {
            if (_ruleFilter.Length > 0
                && violation.RuleLabel != _ruleFilter)
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            return violation.AssetPath.Contains(_search, StringComparison.OrdinalIgnoreCase);
        }

        private int Compare(AssetNamingViolation first, AssetNamingViolation second)
        {
            if (sort == EAssetNamingSort.Name)
                return string.Compare(first.CurrentName, second.CurrentName, StringComparison.OrdinalIgnoreCase);

            if (sort != EAssetNamingSort.Rule)
                return string.Compare(first.AssetPath, second.AssetPath, StringComparison.OrdinalIgnoreCase);

            int byRule = string.Compare(first.RuleLabel, second.RuleLabel, StringComparison.Ordinal);

            return byRule != 0
                ? byRule
                : string.Compare(first.AssetPath, second.AssetPath, StringComparison.OrdinalIgnoreCase);
        }

        private string GroupKeyOf(AssetNamingViolation violation)
        {
            if (sort == EAssetNamingSort.Rule)
                return violation.RuleLabel;

            if (sort != EAssetNamingSort.Folder)
                return string.Empty;

            string directory = Path.GetDirectoryName(violation.AssetPath);

            return string.IsNullOrEmpty(directory)
                ? RootFolder
                : directory.Replace('\\', '/');
        }
    }
}