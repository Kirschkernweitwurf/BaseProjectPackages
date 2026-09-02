using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Adds a pick button next to an object reference. Pressing it arms the scene view, and the next
    /// click there assigns whatever was hit, which beats hunting for the right object in the hierarchy.
    /// </summary>
    /// <remarks>
    /// Only one field can be armed at a time. Pressing the button again, pressing Escape or clicking on
    /// nothing cancels.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SceneViewPickerAttribute : PropertyAttribute { }
}