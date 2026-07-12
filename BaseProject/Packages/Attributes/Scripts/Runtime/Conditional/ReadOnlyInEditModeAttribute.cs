using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Disables the field while not in play mode.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyInEditModeAttribute : PropertyAttribute { }
}