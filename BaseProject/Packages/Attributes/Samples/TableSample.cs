using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A list drawn as a grid.</summary>
    [AttributeSample(typeof(TableAttribute), EAttributeCategory.Collections,
        Description = "Draws a list as a grid with one row per element and one column per field, for elements small "
            + "enough that a stack of foldouts costs more room than it saves.",
        Requirements = "The element type has to be Serializable and small. Past four or five fields the columns stop "
            + "fitting.",
        Variations = new[]
        {
            "TableColumn on the element fields sets their widths and headers."
        })]
    internal sealed class TableSample : ScriptableObject
    {
        [Table]
        [Tooltip("Add a few rows and they line up as a grid.")]
        public List<Row> table = new();

        /// <summary>One row of the table.</summary>
        [Serializable]
        public sealed class Row
        {
            /// <summary>Row name, the widest column.</summary>
            public string id = "Row";

            /// <summary>Numeric column.</summary>
            public int amount = 1;
        }
    }
}