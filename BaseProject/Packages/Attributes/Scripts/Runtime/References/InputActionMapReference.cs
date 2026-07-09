using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.AttributePackage.References
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

        /// <summary>
        /// The GUID of the referenced map. Assigned automatically when a map is selected in the inspector.
        /// </summary>
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