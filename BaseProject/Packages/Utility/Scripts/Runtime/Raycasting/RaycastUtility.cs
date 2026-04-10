using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility.Logging;

namespace Utility.Raycasting
{
    /// <summary>
    /// Provides generic, type-safe ray-casting functionality for 2D gameplay.
    /// Supports editor-only gizmo debugging.
    /// </summary>
    public static class RaycastUtility
    {
#if UNITY_EDITOR
        private const float DebugRayLength = 25f;
        private const float DebugDuration = 1f;

        private static readonly Color DebugRayColor = new(0.1f, 0.8f, 1f, 0.8f);
#endif

        /// <summary>
        /// Attempts to raycast from the main camera at the current mouse position to find a component of type <typeparamref name="T"/>.
        /// Works for 2D gameplay oriented in the XY plane (Z is depth).
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="result">The found component if any.</param>
        /// <returns>True if a hit with the target component was detected; otherwise, false.</returns>
        public static bool TryGetFromMousePosition<T>(out T result)
        {
            result = default;

            Camera cam = Camera.main;
            if (cam == null)
                return false;

            Vector2? pos = Mouse.current?.position.ReadValue();
            if (pos == null)
            {
                CustomLogger.LogWarning("Could not perform raycast from mouse position: Mouse position is null.", null);
                return false;
            }

            Ray ray = cam.ScreenPointToRay(pos.Value);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

#if UNITY_EDITOR
            DrawDebugRay(ray, hit);
#endif

            return hit && hit.collider.TryGetComponent(out result);
        }

        /// <summary>
        /// Performs a raycast using the provided camera and screen point to find a component of type <typeparamref name="T"/>.
        /// Works for 2D gameplay oriented in the XY plane (Z is depth).
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="camera">The camera used to project the ray.</param>
        /// <param name="screenPoint">The screen-space position to cast from.</param>
        /// <param name="result">The found component if any.</param>
        /// <returns>True if a hit with the target component was detected; otherwise, false.</returns>
        public static bool TryGetFromScreenPoint<T>(Camera camera, Vector3 screenPoint, out T result)
        {
            result = default;

            if (camera == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPoint);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

#if UNITY_EDITOR
            DrawDebugRay(ray, hit);
#endif

            return hit && hit.collider.TryGetComponent(out result);
        }

        /// <summary>
        /// Attempts to raycast from the current mouse position to find a UI
        /// element with a component of type <typeparamref name="T"/> within the specified canvas.
        /// </summary>
        /// <param name="graphicRaycaster">The graphic raycaster associated with the target canvas.</param>
        /// <param name="component">The found component if any.</param>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <returns></returns>
        public static bool TryGetUIElement<T>(GraphicRaycaster graphicRaycaster, out T component)
        {
            component = default;

            if (EventSystem.current == null)
            {
                CustomLogger.LogWarning("Could not raycast for UI element: EventSystem is null.", null);
                return false;
            }

            if (graphicRaycaster == null)
            {
                CustomLogger.LogWarning("Could not raycast for UI element: GraphicRaycaster is null.", null);
                return false;
            }

            Vector2? pos = Mouse.current?.position.ReadValue();
            if (pos == null)
            {
                CustomLogger.LogWarning("Could not raycast for UI element: Mouse position is null.", null);
                return false;
            }

            PointerEventData pointer = new(EventSystem.current)
            {
                position = pos.Value
            };

            List<RaycastResult> results = new();
            graphicRaycaster.Raycast(pointer, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.TryGetComponent(out component))
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws debug gizmos for raycast visualization in the Scene view.
        /// </summary>
        private static void DrawDebugRay(Ray ray, RaycastHit2D hit)
        {
            Vector3 start = ray.origin;
            Vector3 end = hit ? hit.point : ray.origin + ray.direction * DebugRayLength;

            Color color = hit ? Color.green : DebugRayColor;
            Debug.DrawLine(start, end, color, DebugDuration);
        }
#endif
    }
}