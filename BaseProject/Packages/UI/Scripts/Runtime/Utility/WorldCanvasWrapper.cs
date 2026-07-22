using Base.AttributePackage;
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
        private void Awake() => canvas.worldCamera = Camera.main;
#endregion
    }
}
