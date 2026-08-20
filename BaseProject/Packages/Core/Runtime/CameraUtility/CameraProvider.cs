using Base.ServicePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Base.CorePackage.CameraUtility
{
    /// <summary>
    /// Centralized access point for cameras.
    /// Caches <see cref="UnityEngine.Camera.main"/> to avoid repeated tag lookups,
    /// and exposes <see cref="UnityEngine.Camera.current"/> without caching it.
    /// </summary>
    public sealed class CameraProvider : GameServiceBehaviour
    {
        private const string MissingCurrentCameraMessage =
            "No rendering camera found. Camera.current is only set during render callbacks "
            + "(OnPreCull, OnPreRender, OnPostRender, OnRenderObject, OnWillRenderObject, "
            + "OnDrawGizmos, OnGUI during repaint).";

        private const string MissingCurrentCameraScriptablePipelineMessage =
            "No rendering camera found. A scriptable render pipeline is active, and "
            + "Camera.current is not populated by URP or HDRP. Use "
            + "RenderPipelineManager.beginCameraRendering to receive the rendering camera instead.";

        private const string MissingMainCameraMessage =
            "No main camera found. Make sure a camera is tagged MainCamera.";

        /// <summary>
        /// The current main camera, or null if none exists.
        /// Resolves lazily and re-resolves automatically if the cached camera was destroyed
        /// (for example after an additive scene unload).
        /// Prefer <see cref="TryGetMain"/> when the caller has to handle the missing case.
        /// </summary>
        public Camera Main
        {
            get
            {
                TryGetMain(out Camera mainCamera);
                return mainCamera;
            }
        }

        /// <summary>
        /// The camera currently rendering, or null if none is.
        /// Only set during render callbacks, and not populated under URP or HDRP.
        /// Never store the result: it changes per camera and per render pass.
        /// Prefer <see cref="TryGetCurrent"/> when the caller has to handle the missing case.
        /// </summary>
        public Camera Current
        {
            get
            {
                TryGetCurrent(out Camera currentCamera);
                return currentCamera;
            }
        }

        /// <summary>Transform of the main camera, or null if none exists.</summary>
        public Transform MainTransform => TryGetMainTransform(out Transform cameraTransform)
            ? cameraTransform
            : null;

        /// <summary>World position of the main camera, or <see cref="Vector3.zero"/> if none exists.</summary>
        public Vector3 MainPosition => TryGetMainPosition(out Vector3 position)
            ? position
            : Vector3.zero;

        private Camera _mainCamera;
        private bool _hasWarnedMain;
        private bool _hasWarnedCurrent;

        /// <summary>
        /// Resolves the main camera and reports whether one exists.
        /// Logs a warning the first time resolving fails, and again after the camera is lost anew.
        /// </summary>
        /// <param name="mainCamera">The resolved camera, or null when this method returns false.</param>
        /// <returns>True if a valid main camera was resolved, otherwise false.</returns>
        public bool TryGetMain(out Camera mainCamera)
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                if (!_hasWarnedMain)
                {
                    CustomLogger.LogWarning(MissingMainCameraMessage, this);
                    _hasWarnedMain = true;
                }

                mainCamera = null;
                return false;
            }

            _hasWarnedMain = false;
            mainCamera = _mainCamera;

            return true;
        }

        /// <summary>
        /// Resolves the camera currently rendering. Read fresh on every call and never cached.
        /// Logs a warning the first time resolving fails, and again after it starts failing anew.
        /// Does not fall back to <see cref="Main"/>: callers that want the main camera must ask for it.
        /// </summary>
        /// <param name="currentCamera">The rendering camera, or null when this method returns false.</param>
        /// <returns>True if a rendering camera was resolved, otherwise false.</returns>
        public bool TryGetCurrent(out Camera currentCamera)
        {
            Camera renderingCamera = Camera.current;

            if (renderingCamera == null)
            {
                if (!_hasWarnedCurrent)
                {
                    CustomLogger.LogWarning(GraphicsSettings.currentRenderPipeline != null
                        ? MissingCurrentCameraScriptablePipelineMessage
                        : MissingCurrentCameraMessage, this);

                    _hasWarnedCurrent = true;
                }

                currentCamera = null;
                return false;
            }

            _hasWarnedCurrent = false;
            currentCamera = renderingCamera;

            return true;
        }

        /// <summary>
        /// Resolves the main camera's transform and reports whether one exists.
        /// </summary>
        /// <param name="cameraTransform">The camera's transform, or null when this method returns false.</param>
        /// <returns>True if a valid main camera was resolved, otherwise false.</returns>
        public bool TryGetMainTransform(out Transform cameraTransform)
        {
            if (!TryGetMain(out Camera mainCamera))
            {
                cameraTransform = null;
                return false;
            }

            cameraTransform = mainCamera.transform;

            return true;
        }

        /// <summary>
        /// Resolves the main camera's world position and reports whether one exists.
        /// </summary>
        /// <param name="position">
        /// The camera's world position, or <see cref="Vector3.zero"/> when this method returns false.
        /// </param>
        /// <returns>True if a valid main camera was resolved, otherwise false.</returns>
        public bool TryGetMainPosition(out Vector3 position)
        {
            if (!TryGetMain(out Camera mainCamera))
            {
                position = Vector3.zero;
                return false;
            }

            position = mainCamera.transform.position;

            return true;
        }

        /// <summary>
        /// Overrides the cached main camera. Useful after additive scene loads
        /// when the desired camera is not the one tagged MainCamera.
        /// </summary>
        /// <param name="mainCamera">The camera to treat as the main camera.</param>
        public void SetMainCamera(Camera mainCamera)
        {
            _mainCamera = mainCamera;
            _hasWarnedMain = false;
        }

        /// <summary>Forces a re-resolve from <see cref="UnityEngine.Camera.main"/>.</summary>
        public void Refresh()
        {
            _mainCamera = Camera.main;
            _hasWarnedMain = false;
        }
    }
}