using Base.AttributePackage;
using Base.CorePackage.CameraUtility;
using Base.CorePackage.Services;
using UnityEngine;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// Wrapper for a world space Canvas to set its world camera to the main camera.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public sealed class WorldCanvasWrapper : MonoBehaviour
    {
        [Tooltip("The canvas that gets the main camera assigned. Auto-assigned from the same GameObject when empty.")]
        [GetComponent] [Required] [SerializeField] private Canvas canvas;

#region Unity Callbacks
        private void Awake()
        {
            if (!ServiceLocator.TryGet(out CameraProvider cameraProvider))
                return;

            canvas.worldCamera = cameraProvider.Main;
        }
#endregion
    }
}