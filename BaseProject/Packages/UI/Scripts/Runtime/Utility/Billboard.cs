using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// A modular component that can be attached to any game object to make it always face the camera,
    /// creating a billboard effect. This is commonly used for UI elements or sprites that need
    /// to remain visible and oriented towards the player regardless of the camera's position.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Locks the billboard to rotate only around the Y axis, keeping it upright. " +
                 "If unchecked, the billboard will always face the camera directly.")]
        [SerializeField] private bool lockYAxis;

        private Camera _targetCamera;

        private void Start()
        {
            if (Camera.main == null)
            {
                CustomLogger.LogWarning("No main camera found for Billboard. " +
                                        "Please assign a camera with the 'MainCamera' tag.", this);
                return;
            }

            _targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (lockYAxis)
            {
                // Only turn horizontally
                Vector3 dir = transform.position - _targetCamera.transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                // Canvas always parallel to camera
                transform.forward = _targetCamera.transform.forward;
            }
        }
    }
}