using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Adds a search box, row labels, alternating row tinting or a delete confirmation to an array or
    /// list, for example <c>[ListDrawerSettings(Searchable = true)]</c>.
    /// </summary>
    /// <remarks>
    /// The list itself stays Unity's own. Everything here is something Unity's list can be told to do
    /// through its callbacks; nothing here replaces its drawing, so a list with this attribute reorders,
    /// selects and resizes exactly like a list without one.
    /// <para>
    /// That rule is what the attribute is for. Anything that would need a second implementation of a
    /// list, paging above all, is deliberately not offered: two renderers that have to look identical
    /// never quite do, and the difference always shows up as a layout bug rather than as a missing
    /// feature.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ListDrawerSettingsAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether a search box filters the rows. Only useful together with
        /// <see cref="LabelMember"/>, since without one the rows are named by index.
        /// </summary>
        public bool Searchable { get; set; }

        /// <summary>Whether removing a row asks first, naming the row it is about to delete.</summary>
        public bool ConfirmDelete { get; set; }

        /// <summary>
        /// Name of a field on the element to label each row with, instead of its index. Use
        /// <c>nameof</c> so a rename carries.
        /// </summary>
        public string LabelMember { get; set; }

        /// <summary>
        /// Whether every other row is tinted, so a long list reads as rows rather than one block of
        /// text. On by default; turn it off for a short list where the tinting is only noise.
        /// </summary>
        public bool ShowAlternatingBackground { get; set; } = true;
    }
}