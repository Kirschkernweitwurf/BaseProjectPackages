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