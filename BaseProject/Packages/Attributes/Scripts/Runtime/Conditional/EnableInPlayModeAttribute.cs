using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Enables the field only while in play mode. The counterpart to
    /// <see cref="ReadOnlyInEditModeAttribute"/> with matching in-play naming.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnableInPlayModeAttribute : PropertyAttribute { }
}
