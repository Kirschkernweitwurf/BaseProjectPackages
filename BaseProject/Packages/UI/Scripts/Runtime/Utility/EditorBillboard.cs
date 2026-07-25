using Base.CorePackage.CameraUtility;
using Base.CorePackage.Services;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Rotates the transform to always face the current viewing camera.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class EditorBillboard : MonoBehaviour
    {
        private Vector3 _lastCameraPos;
        private Quaternion _lastCameraRot;
        private bool _hasCachedCamera;
        private CameraProvider _cameraProvider;

#region Unity Callbacks
        private void Awake()
        {
            if (!Application.isPlaying)
                return;

            ServiceLocator.TryGet(out _cameraProvider);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            if (_cameraProvider == null)
                return;

            if (!_cameraProvider.TryGetMain(out Camera mainCamera))
                return;

            FaceCameraIfMoved(mainCamera);
        }
#endregion

        private void FaceCameraIfMoved(Camera cam)
        {
            if (cam == null)
                return;

            Transform camTransform = cam.transform;
            Vector3 pos = camTransform.position;
            Quaternion rot = camTransform.rotation;

            if (_hasCachedCamera && pos == _lastCameraPos && rot == _lastCameraRot)
                return;

            _lastCameraPos = pos;
            _lastCameraRot = rot;
            _hasCachedCamera = true;

            transform.forward = camTransform.forward;
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView) => FaceCameraIfMoved(sceneView.camera);
#endif
    }
}