using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Measures the screen rect a tooltip is placed beside: the rect of a UI element, or the projected bounds of a
    /// 3D object's renderers, children included, or of its collider.
    /// </summary>
    internal static class TooltipAnchor
    {
        private const int BoxCornerCount = 8;
        private const int DepthBit = 4;
        private const float Half = 0.5f;
        private const int HeightBit = 2;
        private const int RectCornerCount = 4;
        private const int WidthBit = 1;

        private static readonly Vector3[] RectCorners = new Vector3[RectCornerCount];

        /// <summary>A zero-size rect at <paramref name="point"/>, for tooltips that follow the cursor.</summary>
        public static Rect AtPoint(Vector2 point) => new(point, Vector2.zero);

        /// <summary>
        /// The screen rect of <paramref name="target"/>. Falls back to a point at the screen center when the
        /// target has nothing to measure, so the tooltip still shows somewhere readable.
        /// </summary>
        public static Rect Of(Transform target)
        {
            if (target is RectTransform rectTransform)
                return OfRectTransform(rectTransform);

            if (TryGetRendererBounds(target, out Bounds bounds))
                return OfBounds(bounds);

            if (target.TryGetComponent(out Collider collider))
                return OfBounds(collider.bounds);

            return ScreenCenter();
        }

        private static Rect ScreenCenter() => AtPoint(new Vector2(Screen.width, Screen.height) * Half);

        private static Rect OfRectTransform(RectTransform target)
        {
            Camera camera = CameraOf(target.GetComponentInParent<Canvas>());

            target.GetWorldCorners(RectCorners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, RectCorners[0]);
            Vector2 max = min;

            foreach (Vector3 corner in RectCorners)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        // A world space canvas often has no camera of its own, then the main camera shows it.
        private static Camera CameraOf(Canvas canvas)
        {
            if (canvas == null
                || canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            Camera camera = canvas.rootCanvas.worldCamera;

            return camera == null
                ? Camera.main
                : camera;
        }

        // Every visible renderer counts, so a model built from several meshes is measured as a whole.
        private static bool TryGetRendererBounds(Transform target, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled)
                    continue;

                if (!hasBounds)
                    bounds = renderer.bounds;
                else
                    bounds.Encapsulate(renderer.bounds);

                hasBounds = true;
            }

            return hasBounds;
        }

        private static Rect OfBounds(Bounds bounds)
        {
            Camera camera = Camera.main;
            if (camera == null)
                return ScreenCenter();

            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            for (int i = 0; i < BoxCornerCount; i++)
            {
                Vector3 corner = new(Pick(i, WidthBit, bounds.min.x, bounds.max.x),
                    Pick(i, HeightBit, bounds.min.y, bounds.max.y), Pick(i, DepthBit, bounds.min.z, bounds.max.z));

                Vector2 point = camera.WorldToScreenPoint(corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        // Each of the eight box corners takes the min or the max per axis, decided by one bit of its index.
        private static float Pick(int index, int bit, float min, float max) => (index & bit) == 0
            ? min
            : max;
    }
}