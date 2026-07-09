using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Runtime.Builder
{
    /// <summary>
    /// Computes a prefab's bounding box without instantiating it, by walking its
    /// MeshFilter / SkinnedMeshRenderer children and reading shared mesh bounds.
    /// </summary>
    public static class BoundsCalculator
    {
        /// <summary>
        /// Calculates the world-space bounds of a prefab by examining its MeshFilter and SkinnedMeshRenderer components.
        /// Returns a default 1x1x1 bounds at the origin if the prefab is null or has no valid meshes.
        /// Note that this method does not instantiate the prefab, so it may not account for all possible variations
        /// in bounds due to animations, LODs, or other runtime factors.
        /// It provides an approximation based on the static mesh data.
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static Bounds CalculatePrefabBounds(GameObject prefab)
        {
            if (prefab == null)
                return new Bounds(Vector3.zero, Vector3.one);

            bool hasBounds = false;
            Bounds combined = new();
            Transform root = prefab.transform;

            foreach (MeshFilter mf in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null)
                    continue;

                Encapsulate(ref combined, ref hasBounds, mf.transform, root, mf.sharedMesh.bounds);
            }

            foreach (SkinnedMeshRenderer smr in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh == null)
                    continue;

                Encapsulate(ref combined, ref hasBounds, smr.transform, root, smr.sharedMesh.bounds);
            }

            if (!hasBounds)
                CustomLogger.LogWarning($"Prefab '{prefab.name}' has no valid MeshFilter or SkinnedMeshRenderer bounds."
                    + " Returning default bounds.", null);

            return hasBounds
                ? combined
                : new Bounds(Vector3.zero, Vector3.one);
        }

        private static void Encapsulate(ref Bounds combined, ref bool hasBounds,
            Transform source, Transform root, Bounds local)
        {
            Bounds transformed = TransformBoundsToRoot(source, root, local);
            if (!hasBounds)
            {
                combined = transformed;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(transformed);
            }
        }

        private static Bounds TransformBoundsToRoot(Transform source, Transform root, Bounds local)
        {
            Vector3 c = local.center;
            Vector3 e = local.extents;
            Matrix4x4 m = root.worldToLocalMatrix * source.localToWorldMatrix;

            Vector3 p = m.MultiplyPoint3x4(c + new Vector3(-e.x, -e.y, -e.z));
            Bounds result = new(p, Vector3.zero);
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(e.x, -e.y, -e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(-e.x, e.y, -e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(e.x, e.y, -e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(-e.x, -e.y, e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(e.x, -e.y, e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(-e.x, e.y, e.z)));
            result.Encapsulate(m.MultiplyPoint3x4(c + new Vector3(e.x, e.y, e.z)));
            return result;
        }
    }
}