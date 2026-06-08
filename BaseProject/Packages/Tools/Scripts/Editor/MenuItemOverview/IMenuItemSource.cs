using System.Collections.Generic;
using UnityEditor;

namespace Base.ToolPackage.Editor.MenuItemOverview
{
    /// <summary>
    /// Supplies the set of <see cref="MenuItem"/> entries currently defined in the editor.
    /// Implementations decide where that information comes from.
    /// </summary>
    public interface IMenuItemSource
    {
        /// <summary>Builds a fresh snapshot of all menu item entries.</summary>
        IReadOnlyList<MenuItemEntry> Collect();
    }
}