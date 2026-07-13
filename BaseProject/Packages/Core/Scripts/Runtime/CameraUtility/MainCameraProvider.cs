using Base.CorePackage.Services;
using UnityEngine;

// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.CameraUtility
{
    /// <summary>
    /// Centralized access point for the main camera.
    /// Caches <see cref="UnityEngine.Camera.main"/> to avoid repeated tag lookups.
    /// </summary>
    public sealed class MainCameraProvider : GameServiceBehaviour
    {
        /// <summary>
        /// The current main camera. Resolves lazily and re-resolves automatically
        /// if the cached camera was destroyed (for example after an additive scene unload).
        /// </summary>
        public Camera Main
        {
            get
            {
                if (_mainCamera == null)
                    _mainCamera = Camera.main;

                return _mainCamera;
            }
        }

        /// <summary>Transform of the main camera, or null if none exists.</summary>
        public Transform Transform => Main != null
            ? Main.transform
            : null;

        /// <summary>World position of the main camera, or <see cref="Vector3.zero"/> if none exists.</summary>
        public Vector3 Position => Main != null
            ? Main.transform.position
            : Vector3.zero;

        private Camera _mainCamera;

        /// <summary>
        /// Overrides the cached camera. Useful after additive scene loads
        /// when the desired camera is not the one tagged MainCamera.
        /// </summary>
        public void SetMainCamera(Camera mainCamera) => _mainCamera = mainCamera;

        /// <summary>Forces a re-resolve from <see cref="UnityEngine.Camera.main"/>.</summary>
        public void Refresh() => _mainCamera = Camera.main;
    }
}