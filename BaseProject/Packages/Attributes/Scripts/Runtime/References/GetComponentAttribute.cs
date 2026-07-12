using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Auto-assigns the field with a component of the field type on the same GameObject when empty.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetComponentAttribute : PropertyAttribute { }
}