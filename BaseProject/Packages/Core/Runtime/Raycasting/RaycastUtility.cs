using System.Collections.Generic;
using System.Diagnostics;
using Base.CorePackage.CameraUtility;
using Base.ServicePackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// ReSharper disable UnusedMember.Global

namespace Base.CorePackage.Raycasting
{
    /// <summary>
    /// Provides generic, type-safe ray-casting functionality for 2D gameplay.
    /// Supports editor-only debug ray drawing.
    /// </summary>
    public static class RaycastUtility
    {
        private const float DebugDuration = 1f;
        private const float DebugRayLength = 25f;

        private static readonly Color DebugHitColor = Color.green;
        private static readonly Color DebugMissColor = new(0.1f, 0.8f, 1f, 0.8f);

        // Reused across calls to avoid per-raycast list allocations. Safe because calls are main-thread only.
        private static readonly List<RaycastResult> UIRaycastResults = new();

        /// <summary>
        /// Attempts to raycast from the main camera at the current mouse position to find a component of type
        /// <typeparamref name="T"/>.
        /// Works for 2D gameplay oriented in the XY plane (Z is depth).
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="result">The found component if any.</param>
        /// <returns>True if a hit with the target component was detected; otherwise, false.</returns>
        public static bool TryGetFromMousePosition<T>(out T result)
        {
            result = default(T);

            if (!ServiceLocator.TryGet(out CameraProvider cameraProvider))
                return false;

            if (!cameraProvider.TryGetMain(out Camera mainCamera))
                return false;

            if (!TryGetMousePosition(out Vector2 mousePosition))
                return false;

            return TryGetFromRay(mainCamera.ScreenPointToRay(mousePosition), out result);
        }

        /// <summary>
        /// Performs a raycast using the provided camera and screen point to find a component of type
        /// <typeparamref name="T"/>.
        /// Works for 2D gameplay oriented in the XY plane (Z is depth).
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="camera">The camera used to project the ray.</param>
        /// <param name="screenPoint">The screen-space position to cast from.</param>
        /// <param name="result">The found component if any.</param>
        /// <returns>True if a hit with the target component was detected; otherwise, false.</returns>
        public static bool TryGetFromScreenPoint<T>(Camera camera, Vector3 screenPoint, out T result)
        {
            result = default(T);

            if (camera == null)
            {
                CustomLogger.LogWarning($"Could not raycast from screen point: {nameof(camera)} is null.", null);
                return false;
            }

            return TryGetFromRay(camera.ScreenPointToRay(screenPoint), out result);
        }

        /// <summary>
        /// Attempts to raycast from the current mouse position to find a UI
        /// element with a component of type <typeparamref name="T"/> within the specified canvas.
        /// </summary>
        /// <param name="graphicRaycaster">The graphic raycaster associated with the target canvas.</param>
        /// <param name="component">The found component if any.</param>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <returns>True if a hit with the target component was detected; otherwise, false.</returns>
        public static bool TryGetUIElement<T>(GraphicRaycaster graphicRaycaster, out T component)
        {
            component = default(T);

            if (EventSystem.current == null)
            {
                CustomLogger.LogWarning($"Could not raycast for UI element: {nameof(EventSystem)} is null.", null);
                return false;
            }

            if (graphicRaycaster == null)
            {
                CustomLogger.LogWarning($"Could not raycast for UI element: {nameof(graphicRaycaster)} is null.", null);
                return false;
            }

            if (!TryGetMousePosition(out Vector2 mousePosition))
                return false;

            PointerEventData pointer = new(EventSystem.current)
            {
                position = mousePosition
            };

            UIRaycastResults.Clear();
            graphicRaycaster.Raycast(pointer, UIRaycastResults);

            foreach (RaycastResult result in UIRaycastResults)
            {
                if (result.gameObject.TryGetComponent(out component))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Central place for reading the pointer, so a missing device is reported once instead of per call site.
        /// </summary>
        private static bool TryGetMousePosition(out Vector2 position)
        {
            position = default(Vector2);

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                CustomLogger.LogWarning($"Could not perform raycast: no {nameof(Mouse)} device is present.", null);
                return false;
            }

            position = mouse.position.ReadValue();
            return true;
        }

        /// <summary>
        /// Shared cast path, so every entry point behaves and debug-draws identically.
        /// </summary>
        private static bool TryGetFromRay<T>(Ray ray, out T result)
        {
            result = default(T);

            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            DrawDebugRay(ray, hit);

            return hit && hit.collider.TryGetComponent(out result);
        }

        /// <summary>
        /// Editor-only visualization so casts can be followed in the Scene view.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        private static void DrawDebugRay(Ray ray, RaycastHit2D hit)
        {
            Vector3 end = hit
                ? hit.point
                : ray.origin + ray.direction * DebugRayLength;

            Color color = hit
                ? DebugHitColor
                : DebugMissColor;

            Debug.DrawLine(ray.origin, end, color, DebugDuration);
        }
    }
}