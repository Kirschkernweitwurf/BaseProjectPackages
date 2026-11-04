using Base.ToolsPackage.AssetZoo;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Overlay
{
    /// <summary>
    /// Flies the Scene view to parts of the zoo, always from the side the art faces, and from high enough that
    /// nothing tall stands in the way. A zoo stands close together, so flying to a group behind a building would
    /// otherwise end with a view of the building's wall.
    /// </summary>
    internal static class ZooSceneNavigator
    {
        private const float FramePadding = 2f;
        private const float MinViewSize = 1.5f;
        private const float Turn = 180f;
        private const float ViewSizeFactor = 0.42f;
        private const float WholeZooSizeFactor = 0.58f;

        // From a natural look at the art up to nearly a map, the first one with a clear line wins.
        private static readonly float[] Pitches =
        {
            28f,
            42f,
            58f,
            74f
        };

        /// <summary>Shows the whole zoo.</summary>
        public static void FlyToZoo()
        {
            ZooRootMarker root = ZooSceneIndex.Root;

            if (root == null)
                return;

            bool hasBounds = false;
            Bounds bounds = new();

            foreach (ZooGroup group in ZooSceneIndex.Groups)
            {
                if (group == null
                    || !group.gameObject.activeInHierarchy)
                    continue;

                Bounds block = GetBlockBounds(group);

                if (hasBounds)
                {
                    bounds.Encapsulate(block);
                    continue;
                }

                bounds = block;
                hasBounds = true;
            }

            if (hasBounds)
                FlyTo(bounds, root.transform.rotation, WholeZooSizeFactor);
        }

        /// <summary>Shows one group.</summary>
        /// <param name="group">The group to fly to.</param>
        public static void FlyToGroup(ZooGroup group)
        {
            if (group == null)
                return;

            FlyTo(GetBlockBounds(group), group.transform.rotation, ViewSizeFactor);
        }

        /// <summary>Shows one asset.</summary>
        /// <param name="item">The item to fly to.</param>
        public static void FlyToItem(ZooItem item)
        {
            if (item == null)
                return;

            FlyTo(item.WorldBounds, item.transform.rotation, ViewSizeFactor);
        }

        // A zoo is made to be looked at, so with every Scene view closed it opens one instead of doing nothing.
        private static SceneView FindSceneView()
        {
            if (SceneView.lastActiveSceneView != null)
                return SceneView.lastActiveSceneView;

            return SceneView.sceneViews.Count > 0
                ? (SceneView)SceneView.sceneViews[0]
                : EditorWindow.GetWindow<SceneView>();
        }

        private static Bounds GetBlockBounds(ZooGroup group)
        {
            Vector2 size = group.Size;
            Vector3 center = group.transform.TransformPoint(-size.x * 0.5f, 0f, -size.y * 0.5f);

            return new Bounds(center, new Vector3(size.x, 0f, size.y));
        }

        private static void FlyTo(Bounds bounds, Quaternion zooRotation, float sizeFactor)
        {
            SceneView view = FindSceneView();
            float largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) + FramePadding;
            float viewSize = Mathf.Max(MinViewSize, largest * sizeFactor);

            view.in2DMode = false;
            view.LookAt(bounds.center, FindClearView(view, bounds, viewSize, zooRotation), viewSize);
        }

        private static Quaternion FindClearView(SceneView view, Bounds target, float viewSize, Quaternion zooRotation)
        {
            float halfFov = view.cameraSettings.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float distance = viewSize / Mathf.Tan(halfFov);
            Quaternion direction = zooRotation;

            foreach (float pitch in Pitches)
            {
                direction = zooRotation * Quaternion.Euler(pitch, Turn, 0f);
                Vector3 forward = direction * Vector3.forward;

                if (!IsLineBlocked(new Ray(target.center - forward * distance, forward), distance, target))
                    return direction;
            }

            return direction;
        }

        private static bool IsLineBlocked(Ray ray, float distance, Bounds target)
        {
            foreach (ZooItem item in ZooSceneIndex.Items)
            {
                if (item == null
                    || !item.gameObject.activeInHierarchy)
                    continue;

                Bounds bounds = item.WorldBounds;

                // What is being looked at does not block the view of itself.
                if (bounds.Intersects(target))
                    continue;

                if (bounds.IntersectRay(ray, out float hit)
                    && hit < distance)
                    return true;
            }

            return false;
        }
    }
}