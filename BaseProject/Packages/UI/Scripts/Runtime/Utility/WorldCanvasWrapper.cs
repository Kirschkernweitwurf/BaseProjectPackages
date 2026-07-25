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
    public class WorldCanvasWrapper : MonoBehaviour
    {
        [GetComponent] [SerializeField] private Canvas canvas;

#region Unity Callbacks
        private void Awake()
        {
            if (!ServiceLocator.TryGet(out CameraProvider mainCameraProvider))
                return;

            canvas.worldCamera = mainCameraProvider.Main;
        }
#endregion
    }
}