using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Enables the field only while in play mode, for a value worth tuning live and meaningless
    /// before then.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnableInPlayModeAttribute : PropertyAttribute { }
}