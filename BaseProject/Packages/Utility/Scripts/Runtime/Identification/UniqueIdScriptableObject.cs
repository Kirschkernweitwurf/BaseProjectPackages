using System;
using UnityEngine;

namespace Base.UtilityPackage.Identification
{
    /// <summary>
    /// A ScriptableObject that holds a unique ID.
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/UniqueId/UniqueIdScriptableObject", fileName = "UniqueId")]
    public sealed class UniqueIdScriptableObject : ScriptableObject, IUniquelyIdentifiable
    {
        [field: SerializeField, HideInInspector] public string UniqueId { get; private set; }

        /// <summary>
        /// Creates a new instance at runtime with a generated unique ID.
        /// </summary>
        /// <returns>>A new instance with a unique ID.</returns>
        public static UniqueIdScriptableObject CreateRuntime()
        {
            UniqueIdScriptableObject slot = CreateInstance<UniqueIdScriptableObject>();
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