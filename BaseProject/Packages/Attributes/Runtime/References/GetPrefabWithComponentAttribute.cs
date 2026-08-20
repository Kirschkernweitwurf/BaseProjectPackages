using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Fills an empty reference with the first prefab asset in the project carrying the field's
    /// component type. Meant for default prefabs that a system needs a handle on but does not own.
    /// </summary>
    /// <remarks>
    /// Cached and guarded the same way as <see cref="GetScriptableObjectAttribute"/>. On a GameObject
    /// field the prefab root is assigned; on a component field the component on that root is.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetPrefabWithComponentAttribute : PropertyAttribute
    {
        /// <summary>
        /// Component the prefab has to carry. Null uses the field type, which is the usual case and only
        /// needs setting when the field is typed as a GameObject.
        /// </summary>
        public Type ComponentType { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="componentType">Component the prefab has to carry.</param>
        public GetPrefabWithComponentAttribute(Type componentType = null) => ComponentType = componentType;
    }
}