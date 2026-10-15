using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>One field of one asset that points to an asset about to be deleted.</summary>
    internal readonly struct DataReference
    {
        /// <summary>The asset holding the field.</summary>
        internal readonly Object Owner;

        /// <summary>The serialized path of the field.</summary>
        internal readonly string PropertyPath;

        /// <summary>Creates a reference record.</summary>
        /// <param name="owner">The asset holding the field.</param>
        /// <param name="propertyPath">The serialized path of the field.</param>
        internal DataReference(Object owner, string propertyPath)
        {
            Owner = owner;
            PropertyPath = propertyPath;
        }
    }
}