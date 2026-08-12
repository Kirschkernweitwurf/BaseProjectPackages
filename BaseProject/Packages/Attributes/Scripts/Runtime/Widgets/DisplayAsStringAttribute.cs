using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a value as read-only text on one line instead of as an editable control.
    /// </summary>
    /// <remarks>
    /// Not the same as <see cref="ReadOnlyAttribute"/>. Read-only keeps the field exactly as it was and
    /// greys it out, so an array is still a foldout with a row per element. This replaces the field with
    /// a single line of text, so the same array is one row.
    /// <para>
    /// Reach for this when the value is computed and only worth glancing at, and for read-only when the
    /// value is authored somewhere else and the reader may want to inspect it properly.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisplayAsStringAttribute : PropertyAttribute
    {
        /// <summary>Separator used between elements when none is given.</summary>
        public const string DefaultSeparator = ", ";

        /// <summary>Separator drawn between the elements of a collection.</summary>
        public string Separator { get; set; } = DefaultSeparator;
    }
}