using System;
using Base.UtilityPackage.Generated;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.UtilityPackage.Identification
{
    /// <summary>
    /// A ScriptableObject that holds a unique ID.
    /// </summary>
    [CreateAssetMenu(menuName = "Scriptable Objects/Base/UniqueId/UniqueIdScriptableObject", fileName = "UniqueId",
        order = MenuOrders.Code)]
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