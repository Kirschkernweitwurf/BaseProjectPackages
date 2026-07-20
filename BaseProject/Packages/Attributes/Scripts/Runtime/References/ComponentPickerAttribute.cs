using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Marks a component reference field. Dropping a GameObject that holds several components of the
    /// field type opens a picker instead of silently assigning the first one. Fields with several
    /// sibling components show an index badge for telling them apart and swapping, and list elements
    /// offer an entry that adds every matching component at once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ComponentPickerAttribute : PropertyAttribute { }
}