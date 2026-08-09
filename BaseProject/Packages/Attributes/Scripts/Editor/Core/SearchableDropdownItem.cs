using UnityEditor.IMGUI.Controls;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// A leaf entry of a <see cref="SearchableDropdown"/> that remembers which option it stands for, so
    /// the selection can be reported back as an index instead of a display string.
    /// </summary>
    public sealed class SearchableDropdownItem : AdvancedDropdownItem
    {
        /// <summary>Index of the option in the list the dropdown was built from.</summary>
        public int Index { get; }

        /// <summary>Creates a leaf entry.</summary>
        /// <param name="name">The label shown for the entry.</param>
        /// <param name="index">Index of the option this entry stands for.</param>
        public SearchableDropdownItem(string name, int index) : base(name) => Index = index;
    }
}
