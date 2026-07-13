using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>Shows the field only while in play mode.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ShowInPlayModeAttribute : PropertyAttribute { }
}
