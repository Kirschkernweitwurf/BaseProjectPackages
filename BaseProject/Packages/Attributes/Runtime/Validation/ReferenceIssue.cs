using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>A single validation problem found on an object, including the nested path to the field.</summary>
    internal readonly struct ReferenceIssue
    {
        /// <summary>The component or asset that owns the field. Used as log context and ping target.</summary>
        internal readonly Object Owner;

        /// <summary>Dotted path from the owner to the field, for example "level1.level2.field".</summary>
        internal readonly string Path;

        /// <summary>Short reason the field is invalid, for example "is required".</summary>
        internal readonly string Reason;

        /// <summary>Creates an issue record.</summary>
        /// <param name="owner">The component or asset that owns the field.</param>
        /// <param name="path">Dotted path from the owner to the field.</param>
        /// <param name="reason">Short reason the field is invalid.</param>
        internal ReferenceIssue(Object owner, string path, string reason)
        {
            Owner = owner;
            Path = path;
            Reason = reason;
        }
    }
}