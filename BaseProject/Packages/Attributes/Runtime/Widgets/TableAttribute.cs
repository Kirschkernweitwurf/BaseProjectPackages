using System;
using UnityEngine;

namespace Base.AttributesPackage
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
    public sealed class TableAttribute : PropertyAttribute { }
}