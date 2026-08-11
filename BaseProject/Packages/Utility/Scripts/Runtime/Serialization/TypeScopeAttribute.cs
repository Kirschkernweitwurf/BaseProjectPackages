using System;
using UnityEngine;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// Widens a <see cref="TypeReference"/> picker beyond project code, for the rare field that has to
    /// point at a Unity or framework type. Without it the picker offers project types only.
    /// </summary>
    /// <remarks>
    /// A constrained field, <see cref="TypeReferenceOfBase{TBase}"/>, is already narrowed by its base
    /// type and ignores this: everything assignable to that base is worth offering wherever it lives.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TypeScopeAttribute : PropertyAttribute
    {
        /// <summary>Which assemblies the picker draws from.</summary>
        public ETypeScope Scope { get; }

        /// <summary>Creates the attribute.</summary>
        /// <param name="scope">Which assemblies the picker draws from.</param>
        public TypeScopeAttribute(ETypeScope scope = ETypeScope.Everything) => Scope = scope;
    }
}
