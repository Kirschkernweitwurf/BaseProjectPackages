using Base.CorePackage.CameraUtility;
using Base.CorePackage.Services;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Rotates the transform to always face the current viewing camera, in play mode and in the scene view.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class EditorBillboard : MonoBehaviour
    {
        private Vector3 _lastCameraPosition;
        private Quaternion _lastCameraRotation;
        private bool _hasCachedCamera;
        private CameraProvider _cameraProvider;

#region Unity Callbacks
        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            // Without a provider there is nothing to face, so stop instead of failing every frame
            if (!ServiceLocator.TryGet(out _cameraProvider))
                enabled = false;
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            if (!_cameraProvider.TryGetMain(out Camera mainCamera))
                return;

            FaceCameraIfMoved(mainCamera);
        }
#endregion

        private void FaceCameraIfMoved(Camera viewingCamera)
        {
            if (viewingCamera == null)
                return;

            Transform cameraTransform = viewingCamera.transform;
            Vector3 position = cameraTransform.position;
            Quaternion rotation = cameraTransform.rotation;

            // Rotating only on movement keeps the scene view from being marked dirty every repaint
            if (_hasCachedCamera
                && position == _lastCameraPosition
                && rotation == _lastCameraRotation)
                return;

            _lastCameraPosition = position;
            _lastCameraRotation = rotation;
            _hasCachedCamera = true;

            transform.forward = cameraTransform.forward;
        }

#if UNITY_EDITOR
        private void FaceSceneViewCamera(SceneView sceneView) => FaceCameraIfMoved(sceneView.camera);
#endif

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            SceneView.duringSceneGui += FaceSceneViewCamera;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            SceneView.duringSceneGui -= FaceSceneViewCamera;
        }
#endif
    }
}