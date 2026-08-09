using System;
using System.Collections.Generic;
using System.IO;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// One prefab asset found during a scan, together with its place in the variant hierarchy.
    /// </summary>
    public sealed class PrefabEntry
    {
        /// <summary>GUID of the prefab asset.</summary>
        public string Guid { get; }

        /// <summary>Project relative path of the prefab asset.</summary>
        public string AssetPath { get; }

        /// <summary>File name of the prefab without its extension.</summary>
        public string Name { get; }

        /// <summary>Kind of prefab this entry points at.</summary>
        public EPrefabKind Kind { get; }

        /// <summary>GUID of the base prefab, or an empty string when the prefab has no base.</summary>
        public string BaseGuid { get; }

        /// <summary>Number of GameObjects inside the prefab, including the root.</summary>
        public int GameObjectCount { get; }

        /// <summary>Number of components inside the prefab, without the transforms.</summary>
        public int ComponentCount { get; }

        /// <summary>Overrides this variant carries on top of its base. Stays empty for other kinds.</summary>
        public PrefabOverrideCounts Overrides { get; internal set; }

        /// <summary>Entry of the base prefab, or null when the base could not be resolved.</summary>
        public PrefabEntry BaseEntry { get; internal set; }

        /// <summary>Number of steps between this entry and the prefab that starts its variant chain.</summary>
        public int Depth { get; internal set; }

        /// <summary>Number of variants derived from this prefab, direct and indirect.</summary>
        public int TotalVariants { get; internal set; }

        /// <summary>Problems found for this entry.</summary>
        public EPrefabIssue Issues { get; internal set; }

        /// <summary>Variants that use this prefab as their direct base.</summary>
        public IReadOnlyList<PrefabEntry> Children => _children;

        private readonly List<PrefabEntry> _children = new();

        /// <summary>Creates an entry from the data collected while scanning the asset.</summary>
        /// <param name="guid">GUID of the prefab asset.</param>
        /// <param name="assetPath">Project relative path of the prefab asset.</param>
        /// <param name="kind">Kind of prefab the asset is.</param>
        /// <param name="baseGuid">GUID of the base prefab, empty when there is none.</param>
        /// <param name="gameObjectCount">Number of GameObjects inside the prefab.</param>
        /// <param name="componentCount">Number of components inside the prefab, without the transforms.</param>
        public PrefabEntry(string guid,
            string assetPath,
            EPrefabKind kind,
            string baseGuid,
            int gameObjectCount,
            int componentCount)
        {
            Guid = guid;
            AssetPath = assetPath;
            Name = Path.GetFileNameWithoutExtension(assetPath);
            Kind = kind;
            BaseGuid = baseGuid;
            GameObjectCount = gameObjectCount;
            ComponentCount = componentCount;
        }

        internal static int CompareByName(PrefabEntry first, PrefabEntry second)
            => string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);

        internal void AddChild(PrefabEntry child) => _children.Add(child);

        internal void SortChildren() => _children.Sort(CompareByName);
    }
}