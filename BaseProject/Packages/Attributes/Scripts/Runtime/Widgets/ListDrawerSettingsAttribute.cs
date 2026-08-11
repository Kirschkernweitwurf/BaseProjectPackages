using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Replaces Unity's array drawing with a list that can be searched, paged and labeled. Meant for the
    /// lists that outgrow the default drawer: once an array passes a screenful, scrolling to find one
    /// entry stops being workable.
    /// </summary>
    /// <remarks>
    /// Reordering is done with the arrow buttons on each row rather than by dragging, because a dragged
    /// row has no meaning while a search filter or a page is hiding part of the list.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ListDrawerSettingsAttribute : PropertyAttribute
    {
        /// <summary>Page size used when paging is switched on without an explicit size.</summary>
        public const int DefaultPageSize = 20;

        /// <summary>Whether a search box filters the rows by their label.</summary>
        public bool Searchable { get; set; }

        /// <summary>
        /// How many rows are shown at once. Zero shows every row, which is the default.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>Whether the add button is hidden, for lists that are filled from code.</summary>
        public bool HideAddButton { get; set; }

        /// <summary>Whether the remove buttons are hidden.</summary>
        public bool HideRemoveButton { get; set; }

        /// <summary>Whether the reorder arrows are hidden, for lists whose order carries no meaning.</summary>
        public bool HideReorderButtons { get; set; }

        /// <summary>Whether removing a row asks for confirmation first.</summary>
        public bool ConfirmDelete { get; set; }

        /// <summary>
        /// Optional name of a member on the element type used as the row label, so a list of configs
        /// reads as names instead of "Element 0".
        /// </summary>
        public string LabelMember { get; set; }

        /// <summary>Whether the list starts expanded.</summary>
        public bool DefaultExpanded { get; set; } = true;
    }
}
