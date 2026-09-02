using UnityEngine;

namespace Base.ToolsPackage.AssetZoo
{
    /// <summary>
    /// Rotates a generated zoo label so it always faces the viewing camera. In play mode it follows
    /// the main camera on its own; in edit mode the scene view camera is fed in from the editor
    /// assembly, which keeps this component free of editor API and of any package dependency.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden from the Add Component menu; only the zoo builder adds this
    public sealed class ZooLabelBillboard : MonoBehaviour
    {
        private Vector3 _lastCameraPosition;
        private Quaternion _lastCameraRotation;
        private bool _hasCachedCamera;

#region Unity Callbacks
        private void LateUpdate()
        {
            if (!Application.isPlaying)
                return;

            FaceCamera(Camera.main);
        }
#endregion

        /// <summary>
        /// Turns the label toward the given camera. Does nothing while that camera has not moved.
        /// </summary>
        /// <param name="viewingCamera">The camera the label should face.</param>
        public void FaceCamera(Camera viewingCamera)
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
    }
}