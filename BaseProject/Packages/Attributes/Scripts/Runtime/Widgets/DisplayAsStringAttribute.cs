using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a value as read-only text on one line instead of as an editable control.
    /// </summary>
    /// <remarks>
    /// Not the same as <see cref="ReadOnlyAttribute"/>, which keeps the full widget and only greys it
    /// out. A greyed-out array still costs a foldout and a row per element to show something nobody can
    /// change; as text it is one line. Use this for values that are computed, and read-only for values
    /// that are authored elsewhere.
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