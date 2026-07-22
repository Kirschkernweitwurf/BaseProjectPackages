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

#region Unity Callbacks
        private void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            Camera cam = Camera.main;
            if (cam == null)
                cam = Camera.current;

            FaceCameraIfMoved(cam);
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