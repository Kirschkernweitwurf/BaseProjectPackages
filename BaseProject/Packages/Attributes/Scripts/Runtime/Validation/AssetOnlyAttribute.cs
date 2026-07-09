using System;
using UnityEngine;

namespace Base.AttributePackage.Validation
{
    /// <summary>
    /// Restricts an object reference to project assets. Scene object assignments are rejected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AssetOnlyAttribute : PropertyAttribute { }
}