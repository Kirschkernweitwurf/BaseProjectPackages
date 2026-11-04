using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolsPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Finds where a new zoo can stand without running into what is already in the scene, so a scene that holds
    /// hand placed art keeps it, with the zoo next to it.
    /// </summary>
    internal static class ZooPlacement
    {
        private const float FlatHeight = 0.1f;
        private const float FloorMinWidth = 5f;
        private const float Margin = 4f;

        /// <summary>
        /// Finds the corner the zoo starts at. It grows away from there, so it ends up beside the existing art.
        /// </summary>
        /// <param name="scene">The scene the zoo is built into.</param>
        /// <param name="zooRoot">The zoo itself, which does not count as being in the way.</param>
        /// <returns>The near corner of the zoo on the floor.</returns>
        public static Vector3 FindFreeCorner(Scene scene, Transform zooRoot)
        {
            bool hasBounds = false;
            Bounds occupied = new();

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.transform == zooRoot)
                    continue;

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
                {
                    if (zooRoot != null
                        && renderer.transform.IsChildOf(zooRoot))
                        continue;

                    Bounds bounds = renderer.bounds;

                    if (IsFloor(bounds))
                        continue;

                    if (hasBounds)
                    {
                        occupied.Encapsulate(bounds);
                        continue;
                    }

                    occupied = bounds;
                    hasBounds = true;
                }
            }

            return hasBounds
                ? new Vector3(occupied.min.x - Margin, 0f, occupied.max.z)
                : Vector3.zero;
        }

        // A floor is meant to be stood on, not stood next to.
        private static bool IsFloor(Bounds bounds) => bounds.size.y < FlatHeight
            && Mathf.Max(bounds.size.x, bounds.size.z) > FloorMinWidth;
    }
}