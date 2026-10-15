using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Lists
{
    /// <summary>
    /// One distinct entry of a list and how often it appears, for example a card that is in a deck three times.
    /// </summary>
    internal readonly struct DataListGroup
    {
        /// <summary>The asset, or null for empty slots.</summary>
        internal readonly Object Item;

        /// <summary>How many times it appears.</summary>
        internal readonly int Count;

        /// <summary>Creates a group.</summary>
        /// <param name="item">The asset, or null for empty slots.</param>
        /// <param name="count">How many times it appears.</param>
        internal DataListGroup(Object item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}