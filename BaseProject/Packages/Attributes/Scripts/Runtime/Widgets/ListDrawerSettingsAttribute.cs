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
    /// Rows are reordered by dragging the grip on their left. A filter or a page switches that off,
    /// because the row above is then not the element above and a dragged row would land somewhere the
    /// pointer never went.
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

        /// <summary>Whether rows can be dragged. Turn it off for a list whose order carries no meaning.</summary>
        public bool Draggable { get; set; } = true;

        /// <summary>Whether removing a row asks for confirmation first.</summary>
        public bool ConfirmDelete { get; set; }

        /// <summary>
        /// Optional name of a member on the element type used as the row label, so a list of configs
        /// reads as names instead of "Element 0".
        /// </summary>
        public string LabelMember { get; set; }

        /// <summary>Whether the list starts expanded.</summary>
        public bool DefaultExpanded { get; set; } = true;

        /// <summary>
        /// Whether every other row is tinted, so a long list reads as rows rather than as one block of
        /// text. On by default; turn it off for a short list where the striping is only noise.
        /// </summary>
        public bool ShowAlternatingBackground { get; set; } = true;
    }
}