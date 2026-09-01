using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Project local, always writable store for entries that are not part of the shipped
    /// package layout.
    /// </summary>
    [FilePath("ProjectSettings/MenuManagerOverlay.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class MenuOverlay : ScriptableSingleton<MenuOverlay>
    {
        private const int CurrentSchema = 3;

        [SerializeReference]
        private List<MenuNode> menuItemRoot = new();

        [SerializeReference]
        private List<MenuNode> createAssetRoot = new();

        [SerializeField]
        private bool shippedCollapsed;

        [SerializeField]
        private int schemaVersion;

        /// <summary>Whether the shipped, read only section is collapsed in the window.</summary>
        internal bool ShippedCollapsed
        {
            get => shippedCollapsed;
            set => shippedCollapsed = value;
        }

        /// <summary>Returns the top level node list for the given kind.</summary>
        internal List<MenuNode> RootFor(EMenuEntryKind kind) => kind == EMenuEntryKind.CreateAsset
            ? createAssetRoot
            : menuItemRoot;

        /// <summary>Drops unreadable nodes and rebuilds separator flags from retired data.</summary>
        internal void Migrate()
        {
            bool dropped = MenuTree.PruneNulls(menuItemRoot);
            dropped |= MenuTree.PruneNulls(createAssetRoot);

            if (dropped)
                CustomLogger.LogWarning($"Menu Manager: dropped unreadable entries from {nameof(MenuOverlay)}. "
                    + "They are rediscovered on the next scan.", null);

            if (schemaVersion >= CurrentSchema)
            {
                if (dropped)
                    Persist();

                return;
            }

            MenuTree.MigrateSeparators(menuItemRoot);
            MenuTree.MigrateSeparators(createAssetRoot);
            schemaVersion = CurrentSchema;
            Persist();
        }

        /// <summary>Writes the overlay to disk.</summary>
        internal void Persist() => Save(true);
    }
}