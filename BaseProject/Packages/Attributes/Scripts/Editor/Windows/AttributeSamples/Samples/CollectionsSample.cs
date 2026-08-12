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

        [ListDrawerSettings(Searchable = true, PageSize = 4, LabelMember = nameof(Row.id))]
        [Tooltip("A search box and a pager, which is what a genuinely long list wants.")]
        public List<Row> searchable = new();

        [ArraySize(3)]
        [Tooltip("Locks the element count, so the add and remove buttons disappear.")]
        public List<string> exactlyThree = new();

        [Table] public List<Row> table = new();
    }
}