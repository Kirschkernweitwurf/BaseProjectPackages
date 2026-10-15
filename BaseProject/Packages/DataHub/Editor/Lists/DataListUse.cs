using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>One list on one asset that can hold the selection, like the cards of a starter deck.</summary>
    internal readonly struct DataListUse
    {
        /// <summary>The asset holding the list.</summary>
        internal readonly Object Owner;

        /// <summary>The list.</summary>
        internal readonly DataListField Field;

        /// <summary>Creates a use record.</summary>
        /// <param name="owner">The asset holding the list.</param>
        /// <param name="field">The list.</param>
        internal DataListUse(Object owner, DataListField field)
        {
            Owner = owner;
            Field = field;
        }
    }
}