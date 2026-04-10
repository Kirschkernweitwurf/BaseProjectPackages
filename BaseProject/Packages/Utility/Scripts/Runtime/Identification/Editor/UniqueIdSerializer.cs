#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utility.Identification.Editor
{
    /// <summary>
    /// Handles reading/writing the serialized UniqueId string on ScriptableObjects.
    /// Tries both "uniqueId" (the standard field) and the auto-property backing field
    /// </summary>
    internal static class UniqueIdSerializer
    {
        /// <summary>
        /// Gets the UniqueId field from the given asset.
        /// </summary>
        /// <param name="asset">The asset to read.</param>
        /// <returns>The UniqueId string, or null if not found.</returns>
        internal static string GetId(ScriptableObject asset)
        {
            if (asset == null)
                return null;

            SerializedObject so = new(asset);

            // Support both: private [SerializeField] string uniqueId;
            // And: public string UniqueId { get; private set; }
            // which Unity serializes as <UniqueId>k__BackingField
            SerializedProperty p = so.FindProperty("uniqueId") ?? so.FindProperty("<UniqueId>k__BackingField");

            return p?.stringValue;
        }

        /// <summary>
        /// Sets the UniqueId field on the given asset.
        /// </summary>
        /// <param name="asset">The asset to modify.</param>
        /// <param name="newId">The new ID to set.</param>
        internal static void SetId(ScriptableObject asset, string newId)
        {
            if (asset == null)
                return;

            SerializedObject so = new(asset);
            SerializedProperty p = so.FindProperty("uniqueId") ??
                                   so.FindProperty("<UniqueId>k__BackingField");

            if (p == null)
                return; // Asset doesn't actually serialize an ID field, nothing to do.

            p.stringValue = newId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif