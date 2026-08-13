using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a string field as an object picker that stores the path relative to a Resources folder,
    /// ready for Resources.Load. Optionally restrict the picker to a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResourcesAssetAttribute : PropertyAttribute
    {
        /// <summary>Optional type filter for the picker.</summary>
        public Type Type { get; }

        /// <summary>Creates the attribute with an optional type filter.</summary>
        public ResourcesAssetAttribute(Type type = null) => Type = type;
    }
}