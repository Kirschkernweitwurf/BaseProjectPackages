using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Base.ToolsPackage.Editor.MenuManagerModel
{
    /// <summary>Base type for a node in the menu tree. Either a group or an entry.</summary>
    /// <remarks>
    /// The [SerializeReference] records in the shipped registry asset name the namespace and the
    /// assembly this type was written under, so both have to keep resolving after the model was
    /// split off or every stored node loads as null and the asset silently empties itself.
    /// </remarks>
    [Serializable]
    [MovedFrom(false, "Base.ToolsPackage.Editor.MenuManagerWindows",
        "Base.ToolsPackage.Editor", "MenuNode")]
    internal abstract class MenuNode
    {
        [SerializeField]
        private bool separator;

        /// <summary>
        /// When true a priority gap is inserted before this node, which draws a separator
        /// line in the menu.
        /// </summary>
        public bool Separator
        {
            get => separator;
            set => separator = value;
        }
    }
}