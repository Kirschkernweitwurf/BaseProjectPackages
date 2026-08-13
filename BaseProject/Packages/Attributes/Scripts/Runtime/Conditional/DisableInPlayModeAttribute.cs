using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Disables the field while in play mode, for setup that must not change once the game is
    /// running.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DisableInPlayModeAttribute : PropertyAttribute { }
}