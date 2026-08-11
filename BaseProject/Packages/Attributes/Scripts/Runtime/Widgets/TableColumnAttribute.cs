using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Controls how a field of a <see cref="TableAttribute"/> element is presented as a column. Without
    /// it every column gets the same share of the width and the field name as its header.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TableColumnAttribute : PropertyAttribute
    {
        /// <summary>Weight used when none is given, so columns share the width evenly.</summary>
        public const float DefaultWeight = 1f;

        /// <summary>Share of the available width relative to the other columns.</summary>
        public float Weight { get; }

        /// <summary>Optional header text. Null uses the field name.</summary>
        public string Header { get; set; }

        /// <summary>Whether the column is left out of the table entirely.</summary>
        public bool Hidden { get; set; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="weight">Share of the available width relative to the other columns.</param>
        public TableColumnAttribute(float weight = DefaultWeight) => Weight = weight;
    }
}
