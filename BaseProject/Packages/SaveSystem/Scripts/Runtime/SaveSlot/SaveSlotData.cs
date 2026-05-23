using System;
using UnityEngine;
using Base.UtilityPackage.Identification;

namespace Base.SaveSystemPackage.SaveSlot
{
    /// <summary>
    /// Inspector-friendly reference to a save slot. Assign this asset to a button
    /// instead of typing a name string. The id is a stable GUID created once; the
    /// display name is just for the UI and can change at any time.
    /// </summary>
    [CreateAssetMenu(menuName = "Saving/Save Slot", fileName = "SaveSlot")]
    public sealed class SaveSlotData : ScriptableObject, IUniquelyIdentifiable
    {
        [SerializeField, Tooltip("Shown in the UI. Safe to rename anytime.")]
        private string displayName = "New Save";

        [field: SerializeField, HideInInspector] public string UniqueId { get; private set; }

        /// <summary>
        /// User-facing name of this save slot.
        /// This is just for display and can be renamed freely without breaking anything, because the id never changes.
        /// </summary>
        public string DisplayName => displayName;

        /// <summary>
        /// Creates a new instance at runtime with a generated unique ID.
        /// This is useful for save slots that don't need to be assets.
        /// </summary>
        /// <param name="name">The display name for the new save slot.</param>
        /// <returns>>A new instance with a unique ID.</returns>
        public static SaveSlotData CreateRuntime(string name)
        {
            SaveSlotData slot = CreateInstance<SaveSlotData>();
            slot.displayName = name;
            slot.RegenerateId();
            return slot;
        }

        /// <inheritdoc/>
        public void RegenerateId()
        {
            UniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}