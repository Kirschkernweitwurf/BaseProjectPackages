using System;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// A <see cref="TypeReference"/> whose picker only offers types assignable to
    /// <typeparamref name="TBase"/>. The constraint lives in the type argument rather than in an
    /// attribute, so it survives renames and needs no separate drawer.
    /// </summary>
    /// <typeparam name="TBase">The base type or interface every candidate has to satisfy.</typeparam>
    [Serializable]
    public sealed class TypeReferenceOfBase<TBase> : TypeReference
    {
        /// <summary>Creates an empty reference.</summary>
        public TypeReferenceOfBase() { }

        /// <summary>Creates a reference to the given type.</summary>
        /// <param name="type">The type to reference.</param>
        public TypeReferenceOfBase(Type type) : base(type) { }
    }
}
