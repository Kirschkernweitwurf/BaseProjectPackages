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
        [SerializeField] private InputActionAsset asset;
        [SerializeField] private string mapId;

        public InputActionAsset Asset => asset;

        public bool IsValid => asset != null && !string.IsNullOrEmpty(mapId);

        public InputActionMap Resolve() => !IsValid ? null : asset.FindActionMap(mapId);
    }
}