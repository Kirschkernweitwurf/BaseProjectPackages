using System;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>A single validation problem found on an object, including the nested path to the field.</summary>
    public readonly struct ReferenceIssue
    {
        /// <summary>The component or asset that owns the field. Used as log context and ping target.</summary>
        public readonly Object Owner;

        /// <summary>Dotted path from the owner to the field, for example "level1.level2.field".</summary>
        public readonly string Path;

        /// <summary>The declared type of the field.</summary>
        public readonly Type FieldType;

        /// <summary>Short reason the field is invalid, for example "is required".</summary>
        public readonly string Reason;

        /// <summary>Creates an issue record.</summary>
        public ReferenceIssue(Object owner, string path, Type fieldType, string reason)
        {
            Owner = owner;
            Path = path;
            FieldType = fieldType;
            Reason = reason;
        }
    }
}