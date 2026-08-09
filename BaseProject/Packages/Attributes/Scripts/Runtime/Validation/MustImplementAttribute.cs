using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Restricts an object reference to objects that implement the given interfaces or derive from the
    /// given types, for example <c>[MustImplement(typeof(IDamageable))]</c>. Dropping a GameObject
    /// resolves the first component on it that qualifies, so dragging a whole prefab works. Assignments
    /// that cannot be satisfied are reverted and reported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MustImplementAttribute : PropertyAttribute
    {
        /// <summary>The types the assigned object has to be assignable to.</summary>
        public Type[] Types { get; }

        /// <summary>Creates the attribute with the required types.</summary>
        /// <param name="types">The types the assigned object has to be assignable to.</param>
        public MustImplementAttribute(params Type[] types) => Types = types;
    }
}
