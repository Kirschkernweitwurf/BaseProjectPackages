#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Project local, always writable store for entries that are not part of the shipped package layout.</summary>
    [FilePath("ProjectSettings/MenuManagerOverlay.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class MenuOverlay : ScriptableSingleton<MenuOverlay>
    {
        [SerializeReference]
        private List<MenuNode> menuItemRoot = new();

        [SerializeReference]
        private List<MenuNode> createAssetRoot = new();

        [SerializeField]
        private bool shippedCollapsed;

        /// <summary>Whether the shipped, read only section is collapsed in the window.</summary>
        public bool ShippedCollapsed
        {
            get => shippedCollapsed;
            set => shippedCollapsed = value;
        }

        /// <summary>Returns the top level node list for the given kind.</summary>
        public List<MenuNode> RootFor(EMenuEntryKind kind) =>
            kind == EMenuEntryKind.CreateAsset ? createAssetRoot : menuItemRoot;

        /// <summary>Writes the overlay to disk.</summary>
        public void Persist() => Save(true);
    }
}
#endif
