using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Snaps an int field to the nearest power of two, minimum one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PowerOfTwoAttribute : PropertyAttribute { }
}