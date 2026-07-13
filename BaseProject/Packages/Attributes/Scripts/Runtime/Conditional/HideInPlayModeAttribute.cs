using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Hides the field while in play mode.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HideInPlayModeAttribute : PropertyAttribute { }
}
