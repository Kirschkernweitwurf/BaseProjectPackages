using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Adds a clear button at the right edge of an object reference or string field that resets it to
    /// none or empty. The button is disabled while the field is already empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ClearButtonAttribute : PropertyAttribute { }
}