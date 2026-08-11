using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws an array of a serializable type as a grid: one row per element, one column per field.
    /// A list of thirty four-field structs costs thirty rows instead of thirty nested foldouts.
    /// </summary>
    /// <remarks>
    /// Columns are derived from the first element, so an empty list shows only its header until the
    /// first row exists. Use <see cref="TableColumnAttribute"/> on the element's fields to change the
    /// relative width of a column or its header text.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TableAttribute : PropertyAttribute
    {
        /// <summary>Whether the row index is shown in a leading column.</summary>
        public bool ShowRowIndex { get; set; } = true;

        /// <summary>Whether the add button is hidden, for tables that are filled from code.</summary>
        public bool HideAddButton { get; set; }

        /// <summary>Whether the remove buttons are hidden.</summary>
        public bool HideRemoveButton { get; set; }

        /// <summary>Whether the table starts expanded.</summary>
        public bool DefaultExpanded { get; set; } = true;
    }
}
