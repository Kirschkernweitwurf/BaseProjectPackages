using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws an enum as a row of toolbar buttons. Flags enums become multi-select toggles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumToggleButtonsAttribute : PropertyAttribute { }
}