using System;
using Base.UtilityPackage.Menus;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.UtilityPackage.Identification
{
    /// <summary>
    /// A ScriptableObject that holds a unique ID.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/UniqueId/New ScriptableObject", "UID_UniqueIdScriptableObject")]
    public sealed class UniqueIdScriptableObject : ScriptableObject, IUniquelyIdentifiable
    {
        [field: SerializeField] [field: HideInInspector] public string UniqueId { get; private set; }

        /// <inheritdoc/>
        public void RegenerateId()
        {
            UniqueId = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}