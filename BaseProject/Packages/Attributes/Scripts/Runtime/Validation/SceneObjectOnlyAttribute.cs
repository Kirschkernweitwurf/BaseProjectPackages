using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Restricts an object reference to scene objects. Project asset assignments are rejected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SceneObjectOnlyAttribute : PropertyAttribute { }
}