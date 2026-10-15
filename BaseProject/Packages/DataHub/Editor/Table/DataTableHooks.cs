using System;
using UnityEngine;

namespace Base.DataHubPackage.Editor.Table
{
    /// <summary>
    /// What the table asks the window to do. The table knows where the user clicked, the window knows
    /// how to create, rename and filter, so the table only raises these.
    /// </summary>
    internal sealed class DataTableHooks
    {
        /// <summary>Clears the search and the problem filter, from the button of an empty result.</summary>
        internal Action ClearFilter { get; set; }

        /// <summary>Creates a new asset, from the button of an empty table.</summary>
        internal Action Create { get; set; }

        /// <summary>Renames the selected asset, after a double click on its name or F2.</summary>
        internal Action Rename { get; set; }

        /// <summary>Opens the menu of a column after a right click on its title, given in screen space.</summary>
        internal Action<DataColumn, Rect> ShowColumnMenu { get; set; }

        /// <summary>Opens the row menu, after a right click selected the row.</summary>
        internal Action ShowContextMenu { get; set; }
    }
}