using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Renders an enum as a multi-select mask field. Use on enums marked with <see cref="FlagsAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EnumFlagsAttribute : PropertyAttribute { }
}