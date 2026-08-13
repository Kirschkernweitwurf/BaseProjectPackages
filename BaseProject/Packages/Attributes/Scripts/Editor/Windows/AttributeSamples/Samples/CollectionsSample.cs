using System;
using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Lists and tables.</summary>
    [AttributeSample("Collections")]
    internal sealed class CollectionsSample : ScriptableObject
    {
        /// <summary>One row, shaped so the table has something to make columns from.</summary>
        [Serializable]
        public sealed class Row
        {
            /// <summary>Row name, used as the list label and the widest column.</summary>
            [TableColumn(2f)] public string id = "Row";

            /// <summary>Numeric column with its own header.</summary>
            [TableColumn(Header = "Qty")] public int amount = 1;

            /// <summary>Object column.</summary>
            [TableColumn(1.5f)] public Material material;
        }

        [InfoBox("Drag a row by the grip on its left. Add and remove live in the footer.")]
        [Tooltip("A plain list with no attribute, drawn by Unity, for comparison with the ones below.")]
        public List<string> plain = new();

        [ListDrawerSettings(LabelMember = nameof(Row.id))]
        [Tooltip("Names each row after a field on the element instead of calling it Element 0.")]
        public List<Row> labeled = new();

        [ListDrawerSettings(Searchable = true, LabelMember = nameof(Row.id))]
        [Tooltip("A search box that hides the rows whose label does not match. Dragging switches off "
            + "while it is filtering, because the row above is then not the element above.")]
        public List<Row> searchable = new();

        [ArraySize(3)]
        [Tooltip("Locks the element count, so the add and remove buttons disappear.")]
        public List<string> exactlyThree = new();

        [ListDrawerSettings(ConfirmDelete = true, LabelMember = nameof(Row.id))]
        [Tooltip("Removing a row asks first, naming the row it is about to delete.")]
        public List<Row> confirmed = new();

        [ListDrawerSettings(ShowAlternatingBackground = false)]
        [Tooltip("Row tinting turned off. On by default, and worth keeping for anything longer than a "
            + "handful of rows.")]
        public List<string> plainRows = new();

        [Table] public List<Row> table = new();
    }
}