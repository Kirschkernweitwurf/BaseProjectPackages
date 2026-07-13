using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Disables the field while in play mode. The counterpart to
    /// <see cref="ReadOnlyInPlayModeAttribute"/> with matching in-play naming.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisableInPlayModeAttribute : PropertyAttribute { }
}
