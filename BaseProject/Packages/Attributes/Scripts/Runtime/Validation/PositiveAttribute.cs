using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Clamps an int or float field to zero or above. Negative values are reset to zero.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PositiveAttribute : PropertyAttribute { }
}