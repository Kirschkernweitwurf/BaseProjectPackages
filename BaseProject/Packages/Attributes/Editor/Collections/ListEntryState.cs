using System.Collections.Generic;

namespace Base.AttributePackage.Editor.Collections
{
    /// <summary>
    /// What a reorderable list needs to know about the field it is drawing, refreshed on every draw.
    /// </summary>
    /// <remarks>
    /// Held beside the list rather than captured by its callbacks. The callbacks are built once and the
    /// settings can change between repaints, so anything captured at construction would be the settings
    /// as they were the first time the field was ever drawn.
    /// </remarks>
    internal readonly struct ListEntryState
    {

        /// <summary>Whether removing a row asks first.</summary>
        internal readonly bool ConfirmDelete;

        /// <summary>Whether every other row is tinted.</summary>
        internal readonly bool Striped;

        private readonly HashSet<int> _hidden;

        /// <summary>Captures the settings for one draw.</summary>
        /// <param name="settings">The settings on the field.</param>
        /// <param name="hidden">Indices the filter is hiding this draw.</param>
        public ListEntryState(ListDrawerSettingsAttribute settings, HashSet<int> hidden)
        {
            ConfirmDelete = settings.ConfirmDelete;
            Striped = settings.ShowAlternatingBackground;
            _hidden = hidden;
        }

        /// <summary>Whether the filter is hiding the given row.</summary>
        /// <param name="index">Index of the row.</param>
        /// <returns>True while the row is filtered out.</returns>
        internal bool IsHidden(int index) => _hidden != null && _hidden.Contains(index);
    }
}