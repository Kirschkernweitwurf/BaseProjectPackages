using UnityEditor;
using UnityEngine;

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

            Camera cam = Camera.main != null
                ? Camera.main
                : Camera.current;

            FaceCameraIfMoved(cam);
        }
#endregion

        private void FaceCameraIfMoved(Camera cam)
        {
            if (cam == null)
                return;

            Transform camT = cam.transform;
            Vector3 pos = camT.position;
            Quaternion rot = camT.rotation;

            if (_hasCachedCamera && pos == _lastCameraPos && rot == _lastCameraRot)
                return;

            _lastCameraPos = pos;
            _lastCameraRot = rot;
            _hasCachedCamera = true;

            transform.forward = camT.forward;
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