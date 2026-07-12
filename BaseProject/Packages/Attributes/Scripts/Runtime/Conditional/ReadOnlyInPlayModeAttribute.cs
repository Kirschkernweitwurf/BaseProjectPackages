using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Disables the field while in play mode.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReadOnlyInPlayModeAttribute : PropertyAttribute { }
}