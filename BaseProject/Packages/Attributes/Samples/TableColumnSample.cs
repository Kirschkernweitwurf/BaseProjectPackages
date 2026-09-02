using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>One column of a table, sized and named.</summary>
    [AttributeSample(typeof(TableColumnAttribute), EAttributeCategory.Collections,
        Description = "Sets how wide one column of a table is relative to the others, and can give it a header that "
            + "differs from the field name.",
        Requirements = "Only read while the list is drawn as a table. On a list without the table attribute it does "
            + "nothing.",
        Variations = new[]
        {
            "A width argument sets the relative width, where one is the default share.",
            "Header renames the column."
        })]
    internal sealed class TableColumnSample : ScriptableObject
    {
        [Table]
        [Tooltip("Add a few rows to see the column widths below take effect.")]
        public List<Row> table = new();

        /// <summary>One row, with each column sized and named.</summary>
        [Serializable]
        public sealed class Row
        {
            /// <summary>Twice the default width.</summary>
            [TableColumn(2f)] public string id = "Row";

            /// <summary>Default width, renamed header.</summary>
            [TableColumn(Header = "Qty")] public int amount = 1;

            /// <summary>One and a half times the default width.</summary>
            [TableColumn(1.5f)] public Material material;
        }
    }
}