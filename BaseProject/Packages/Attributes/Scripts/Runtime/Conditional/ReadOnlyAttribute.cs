using System;
using UnityEngine;

namespace Base.AttributePackage.Conditional
{
    /// <summary>
    /// Marks a serialized field as non-editable in the inspector while keeping it visible.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyAttribute : PropertyAttribute { }
}