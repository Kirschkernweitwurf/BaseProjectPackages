using System;
using UnityEngine;

namespace Base.AttributePackage.Conditional
{
    /// <summary>Disables the field while in play mode.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyInPlayModeAttribute : PropertyAttribute { }
}