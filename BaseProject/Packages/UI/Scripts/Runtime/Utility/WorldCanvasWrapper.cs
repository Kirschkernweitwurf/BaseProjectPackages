using UnityEngine;

namespace UI.Utility
{
    /// <summary>
    /// Wrapper for a world space Canvas to set its world camera to the main camera.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class WorldCanvasWrapper : MonoBehaviour
    {
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = Camera.main;
        }
    }
}