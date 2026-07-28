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
    public sealed class Billboard : MonoBehaviour
    {
        private const float MinFacingDistanceSqr = 0.001f;

        [Header("Settings")]

        [Tooltip("Locks the billboard to rotate only around the Y axis, keeping it upright. "
            + "If unchecked, the billboard will always face the camera directly.")]
        [SerializeField] private bool lockYAxis;

        private CameraProvider _cameraProvider;

#region Unity Callbacks
        private void Awake()
        {
            // Without a provider there is nothing to face, so stop instead of failing every frame
            if (!ServiceLocator.TryGet(out _cameraProvider))
                enabled = false;
        }

        private void LateUpdate()
        {
            if (!_cameraProvider.TryGetMainTransform(out Transform cameraTransform))
                return;

            if (!lockYAxis)
            {
                // Canvas always parallel to camera
                transform.forward = cameraTransform.forward;
                return;
            }

            // Only turn horizontally
            Vector3 direction = transform.position - cameraTransform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinFacingDistanceSqr)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }
#endregion
    }
}