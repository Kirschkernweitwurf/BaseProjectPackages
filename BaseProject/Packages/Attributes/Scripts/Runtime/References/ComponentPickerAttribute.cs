using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Marks a component reference field. Dropping a GameObject that holds several components of the
    /// field type opens a picker instead of silently assigning the first one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ComponentPickerAttribute : PropertyAttribute
    {
        /// <summary>Offers a menu entry that fills the whole list with every matching component.</summary>
        public readonly bool AllowFillList;

        /// <summary>Draws a small index badge so identical components can be told apart and swapped.</summary>
        public readonly bool ShowIndexBadge;

        /// <summary>Creates a new picker attribute.</summary>
        /// <param name="allowFillList">Allows adding all matching components at once when used on a list.</param>
        /// <param name="showIndexBadge">Draws the sibling index next to the field.</param>
        public ComponentPickerAttribute(bool allowFillList = true, bool showIndexBadge = true)
        {
            AllowFillList = allowFillList;
            ShowIndexBadge = showIndexBadge;
        }
    }
}