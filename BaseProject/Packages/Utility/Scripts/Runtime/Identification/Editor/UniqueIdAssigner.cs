#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utility.Identification.Editor
{
    /// <summary>
    /// Ensures a specific <see cref="ScriptableObject"/> that implements <see cref="IUniquelyIdentifiable"/>
    /// has a valid, globally unique ID. If it had no ID or a duplicate, we assign a new one.
    /// </summary>
    internal static class UniqueIdAssigner
    {
        /// <summary>
        /// Ensures the given asset has a valid unique ID.
        /// </summary>
        /// <param name="asset">The asset to check.</param>
        internal static void EnsureForAsset(ScriptableObject asset)
        {
            if (asset == null)
                return;

            if (asset is not IUniquelyIdentifiable)
                return;

            string currentId = UniqueIdSerializer.GetId(asset);

            string finalId = UniqueIdRegistry.EnsureUnique(asset, currentId);
            if (finalId == currentId)
                return;

            UniqueIdSerializer.SetId(asset, finalId);
            EditorUtility.SetDirty(asset);
        }
    }
}
#endif