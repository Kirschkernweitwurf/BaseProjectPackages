using System;
using System.Collections.Generic;
using System.IO;

namespace Base.ToolPackage.Editor.OverviewGui.PrefabOverviewWindow
{
    /// <summary>
    /// One prefab asset found during a scan, together with its place in the variant hierarchy.
    /// </summary>
    internal sealed class PrefabEntry
    {
        /// <summary>GUID of the prefab asset.</summary>
        internal string Guid { get; }

        /// <summary>Project relative path of the prefab asset.</summary>
        internal string AssetPath { get; }

        /// <summary>File name of the prefab without its extension.</summary>
        internal string Name { get; }

        /// <summary>Kind of prefab this entry points at.</summary>
        internal EPrefabKind Kind { get; }

        /// <summary>GUID of the base prefab, or an empty string when the prefab has no base.</summary>
        internal string BaseGuid { get; }

        /// <summary>Number of GameObjects inside the prefab, including the root.</summary>
        internal int GameObjectCount { get; }

        /// <summary>Number of components inside the prefab, without the transforms.</summary>
        internal int ComponentCount { get; }

        /// <summary>Overrides this variant carries on top of its base. Stays empty for other kinds.</summary>
        internal PrefabOverrideCounts Overrides { get; set; }

        /// <summary>Entry of the base prefab, or null when the base could not be resolved.</summary>
        internal PrefabEntry BaseEntry { get; set; }

        /// <summary>Number of steps between this entry and the prefab that starts its variant chain.</summary>
        internal int Depth { get; set; }

        /// <summary>Number of variants derived from this prefab, direct and indirect.</summary>
        internal int TotalVariants { get; set; }

        /// <summary>Problems found for this entry.</summary>
        internal EPrefabIssue Issues { get; set; }

        /// <summary>Variants that use this prefab as their direct base.</summary>
        internal IReadOnlyList<PrefabEntry> Children => _children;

        private readonly List<PrefabEntry> _children = new();

        /// <summary>Creates an entry from the data collected while scanning the asset.</summary>
        /// <param name="guid">GUID of the prefab asset.</param>
        /// <param name="assetPath">Project relative path of the prefab asset.</param>
        /// <param name="kind">Kind of prefab the asset is.</param>
        /// <param name="baseGuid">GUID of the base prefab, empty when there is none.</param>
        /// <param name="gameObjectCount">Number of GameObjects inside the prefab.</param>
        /// <param name="componentCount">Number of components inside the prefab, without the transforms.</param>
        public PrefabEntry(string guid, string assetPath, EPrefabKind kind, string baseGuid, int gameObjectCount,
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