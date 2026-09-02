using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Adds a button at the right edge of the field that copies the current value to the system
    /// clipboard as text. The button is disabled while the value is empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CopyButtonAttribute : PropertyAttribute { }
}