using Base.CorePackage.CameraUtility;
using Base.CorePackage.Services;
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

        private CameraProvider _cameraProvider;

#region Unity Callbacks
        private void Awake() => ServiceLocator.TryGet(out _cameraProvider);

        private void LateUpdate()
        {
            if (!_cameraProvider.TryGetMainTransform(out Transform cameraTransform))
                return;

            if (lockYAxis)
            {
                // Only turn horizontally
                Vector3 direction = transform.position - cameraTransform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Canvas always parallel to camera
                transform.forward = cameraTransform.forward;
            }
        }
#endregion
    }
}