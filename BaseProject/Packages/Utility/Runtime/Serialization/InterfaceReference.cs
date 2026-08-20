using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// Serializes a reference to a Unity object while exposing it through an interface. Unity cannot
    /// serialize interface-typed fields, so the underlying object is stored and the interface is
    /// resolved on access. The inspector restricts assignment to objects that implement the interface.
    /// </summary>
    /// <remarks>
    /// Not sealed on purpose: <see cref="InterfaceReference{TInterface}"/> derives from it to offer the
    /// common single-parameter form.
    /// </remarks>
    /// <typeparam name="TInterface">The interface the reference is used through.</typeparam>
    /// <typeparam name="TObject">The Unity object type accepted by the field.</typeparam>
    [Serializable]
    public class InterfaceReference<TInterface, TObject>
        where TInterface : class
        where TObject : Object
    {
        /// <summary>Name of the serialized object field. Used by the inspector drawer.</summary>
        public const string UnderlyingField = nameof(underlyingValue);

        [SerializeField] private TObject underlyingValue;

        /// <summary>
        /// The referenced object as the interface, or null when nothing is assigned or the object was
        /// destroyed. Assigning a value that is not a <typeparamref name="TObject"/> clears the field.
        /// </summary>
        public TInterface Value
        {
            get => UnityObjectUtility.IsAlive(underlyingValue)
                ? underlyingValue as TInterface
                : null;
            set => underlyingValue = value as TObject;
        }

        /// <summary>The referenced object itself, for cases that need the Unity object.</summary>
        public TObject UnderlyingValue => underlyingValue;

        /// <summary>Creates an empty reference.</summary>
        public InterfaceReference() { }

        /// <summary>Creates a reference to the given object.</summary>
        /// <param name="target">The object to reference.</param>
        public InterfaceReference(TObject target) => underlyingValue = target;
    }
}