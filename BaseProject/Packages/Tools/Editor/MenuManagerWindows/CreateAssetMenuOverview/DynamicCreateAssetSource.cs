using System;
using System.Collections.Generic;
using Base.ToolsPackage.Editor.MenuManagerModel;
using Base.ToolsPackage.Editor.Shared;
using UnityEditor;

namespace Base.ToolsPackage.Editor.MenuManagerWindows.CreateAssetMenuOverview
{
    /// <summary>
    /// Source that reports every asset creation entry registered through the menu manager,
    /// including the ones that are switched off or whose type has disappeared since the last scan.
    /// </summary>
    internal sealed class DynamicCreateAssetSource : ICreateAssetSource
    {
        private readonly MenuScriptLookup _scripts = new();

        /// <inheritdoc/>
        public IReadOnlyList<CreateAssetEntry> Collect()
        {
            _scripts.Clear();
            List<CreateAssetEntry> entries = new();

            Dictionary<string, ResolvedMenu> resolved = MenuScanner.Scan();
            MenuComposite.Recalculate();

            foreach ((MenuEntry entry, string path) in MenuComposite.ResolvedEntries(EMenuEntryKind.CreateAsset))
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                entries.Add(Build(entry, path, resolved));
            }

            return entries;
        }

        // Managed paths carry the fixed "Assets/Create" root, attributed ones do not. Drop it so
        // both kinds line up in the same column.
        private static string Relative(string path)
        {
            string root = MenuPath.AssetRoot + "/";

            return path.StartsWith(root, StringComparison.Ordinal)
                ? path[root.Length..]
                : path;
        }

        // Ids are built as "CA:{Type.FullName}", so the type name is the last segment.
        private static string TypeNameOf(string entryId, Type declaringType)
        {
            if (declaringType != null)
                return declaringType.Name;

            if (string.IsNullOrEmpty(entryId))
                return string.Empty;

            string full = entryId.StartsWith(MenuEntryId.CreateAssetPrefix, StringComparison.Ordinal)
                ? entryId[MenuEntryId.CreateAssetPrefix.Length..]
                : entryId;

            int separator = full.LastIndexOf('.');
            return separator >= 0
                ? full[(separator + 1)..]
                : full;
        }

        private static EMenuEntryState StateOf(MenuEntry entry)
        {
            if (entry.Missing)
                return EMenuEntryState.Missing;

            return entry.Enabled
                ? EMenuEntryState.Active
                : EMenuEntryState.Disabled;
        }

        private CreateAssetEntry Build(MenuEntry entry, string path, IReadOnlyDictionary<string, ResolvedMenu> resolved)
        {
            resolved.TryGetValue(entry.Id, out ResolvedMenu match);
            Type declaringType = match?.AssetType;

            MonoScript script = _scripts.Resolve(declaringType);
            string assetPath = MenuScriptLookup.PathOf(script);
            EAssetOrigin origin = AssetOriginResolver.Classify(assetPath);

            string fileName = string.IsNullOrWhiteSpace(entry.CreateFileName)
                ? match?.DefaultFileName
                : entry.CreateFileName;

            return CreateAssetEntry.Managed(entry.Id, Relative(path), fileName, declaringType,
                TypeNameOf(entry.Id, declaringType), entry.EffectivePriority, StateOf(entry), origin, script,
                assetPath);
        }
    }
}