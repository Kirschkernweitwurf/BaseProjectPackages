#if UNITY_EDITOR
using System;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Base type for a node in the menu tree. Either a group or an entry.</summary>
    [Serializable]
    public abstract class MenuNode
    {
        [SerializeField]
        private bool separator;

        /// <summary>When true a priority gap is inserted before this node, which draws a separator line in the menu.</summary>
        public bool Separator
        {
            get => separator;
            set => separator = value;
        }
    }
}
#endif