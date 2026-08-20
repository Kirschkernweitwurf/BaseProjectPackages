using System;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// The common form of <see cref="InterfaceReference{TInterface, TObject}"/> that accepts any Unity
    /// object implementing the interface, so only the interface has to be named at the use site.
    /// </summary>
    /// <typeparam name="TInterface">The interface the reference is used through.</typeparam>
    [Serializable]
    public sealed class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object>
        where TInterface : class
    {
        /// <summary>Creates an empty reference.</summary>
        public InterfaceReference() { }

        /// <summary>Creates a reference to the given object.</summary>
        /// <param name="target">The object to reference.</param>
        public InterfaceReference(Object target) : base(target) { }
    }
}