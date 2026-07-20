using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Marks a component reference field. Dropping a GameObject assigns the first matching component,
    /// and the index badge next to the assigned field opens a picker for swapping to any other sibling
    /// component of the field type. On list elements the picker also offers adding every matching
    /// component at once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ComponentPickerAttribute : PropertyAttribute { }
}