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

        [Tooltip("Locks the billboard to rotate only around the Y axis, keeping it upright. "
            + "If unchecked, the billboard will always face the camera directly.")]
        [SerializeField] private bool lockYAxis;

        private Transform _cameraTransform;

#region Unity Callbacks
        private void Start()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                CustomLogger.LogWarning("No main camera found for Billboard. "
                    + "Please assign a camera with the 'MainCamera' tag.", this);

                enabled = false;
                return;
            }

            _cameraTransform = mainCamera.transform;
        }

        private void LateUpdate()
        {
            if (lockYAxis)
            {
                // Only turn horizontally
                Vector3 direction = transform.position - _cameraTransform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Canvas always parallel to camera
                transform.forward = _cameraTransform.forward;
            }
        }
#endregion
    }
}
