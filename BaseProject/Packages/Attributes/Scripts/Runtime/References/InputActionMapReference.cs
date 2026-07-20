using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.AttributePackage
{
    /// <summary>
    /// Serializable reference to an <see cref="InputActionMap"/> inside an
    /// <see cref="InputActionAsset"/>. Stores the map's GUID, so it survives renames.
    /// </summary>
    /// <example>
    /// <code>
    /// public class Example : MonoBehaviour
    /// {
    ///     public InputActionMapReference actionMapRef;
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public struct InputActionMapReference
    {
        /// <summary>Serialized name of the asset field, for use by the property drawer.</summary>
        public const string AssetFieldName = nameof(asset);

        /// <summary>Serialized name of the map id field, for use by the property drawer.</summary>
        public const string MapIdFieldName = nameof(mapId);

        [SerializeField] private InputActionAsset asset;
        [SerializeField] private string mapId;

        /// <summary>
        /// The asset containing the referenced map. Must be assigned for the reference to be valid.
        /// </summary>
        public InputActionAsset Asset => asset;

        /// <summary>
        /// The GUID of the referenced map. Assigned automatically when a map is selected in the inspector.
        /// </summary>
        public string MapId => mapId;

        /// <summary>Whether the reference points at an assigned asset and a selected map.</summary>
        public bool IsValid => asset != null && !string.IsNullOrEmpty(mapId);

        /// <summary>
        /// Resolves the reference to an actual <see cref="InputActionMap"/> instance.
        /// Returns <c>null</c> if the reference is invalid.
        /// </summary>
        /// <returns>The resolved <see cref="InputActionMap"/>, or null if the reference is invalid.</returns>
        public InputActionMap Resolve() => !IsValid
            ? null
            : asset.FindActionMap(mapId);
    }
}