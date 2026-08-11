using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a bool with the checkbox in front of the label instead of behind it, which reads better in
    /// a column of options than Unity's default placement.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LeftToggleAttribute : PropertyAttribute { }
}