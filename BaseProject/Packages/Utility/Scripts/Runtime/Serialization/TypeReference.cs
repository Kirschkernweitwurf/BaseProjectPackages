using System;
using UnityEngine;

namespace Base.UtilityPackage.Serialization
{
    /// <summary>
    /// Serializes a <see cref="Type"/>, which Unity cannot store on its own. The assembly qualified name
    /// is what actually persists; the resolved type is cached and rebuilt after every deserialization.
    /// </summary>
    /// <remarks>
    /// Not sealed on purpose: <see cref="TypeReference{TBase}"/> derives from it to constrain the picker
    /// to a base type without needing an attribute.
    /// <para>
    /// A stored type that no longer exists resolves to null rather than throwing. Renaming or moving a
    /// type breaks the reference the same way it breaks any other name-based serialization, so treat
    /// this like a scene name, not like a GUID.
    /// </para>
    /// </remarks>
    [Serializable]
    public class TypeReference : ISerializationCallbackReceiver
    {
        /// <summary>Name of the serialized field. Used by the inspector drawer.</summary>
        public const string TypeNameField = nameof(typeName);

        [SerializeField] private string typeName;

        private Type _resolved;
        private bool _isResolved;

        /// <summary>The referenced type, or null when nothing is set or the type no longer exists.</summary>
        public Type Value
        {
            get
            {
                if (!_isResolved)
                    Resolve();

                return _resolved;
            }
            set
            {
                typeName = value == null
                    ? string.Empty
                    : value.AssemblyQualifiedName;

                _resolved = value;
                _isResolved = true;
            }
        }

        /// <summary>True when a name is stored but no longer resolves to a type.</summary>
        public bool IsBroken => !string.IsNullOrEmpty(typeName) && Value == null;

        /// <summary>Creates an empty reference.</summary>
        public TypeReference() { }

        /// <summary>Creates a reference to the given type.</summary>
        /// <param name="type">The type to reference.</param>
        public TypeReference(Type type) => Value = type;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        // The cached type is dropped rather than re-resolved here, because deserialization can run on a
        // background thread where reflection over loaded assemblies is not safe.
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _resolved = null;
            _isResolved = false;
        }

        /// <summary>Returns the referenced type name, for logs and inspectors.</summary>
        /// <returns>The short type name, or an empty marker.</returns>
        public override string ToString()
        {
            Type value = Value;

            return value == null
                ? string.Empty
                : value.Name;
        }

        private void Resolve()
        {
            _resolved = string.IsNullOrEmpty(typeName)
                ? null
                : Type.GetType(typeName);

            _isResolved = true;
        }
    }
}
