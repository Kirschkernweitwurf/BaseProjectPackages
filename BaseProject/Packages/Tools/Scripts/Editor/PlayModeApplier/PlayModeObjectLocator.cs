using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Finds an object again by where it sits in a hierarchy rather than by id.
    /// GlobalObjectId cannot be used for this: play mode unpacks every prefab instance, which rewrites its id,
    /// so an id captured in play mode never resolves back in edit mode.
    /// Each path is stored twice, once by sibling index and once by name. The index is tried first and the
    /// name is used to verify it, so a renamed object or a reordered hierarchy still resolves.
    /// </summary>
    public static class PlayModeObjectLocator
    {
        private const char SegmentSeparator = '/';
        private const int UnknownIndex = -1;

        /// <summary>Builds a name path from the scene root down to the target, root included.</summary>
        public static string BuildSceneNamePath(Transform target)
        {
            List<string> parts = new();
            Transform cursor = target;

            while (cursor != null)
            {
                parts.Add(cursor.name);
                cursor = cursor.parent;
            }

            parts.Reverse();
            return string.Join(SegmentSeparator.ToString(), parts);
        }

        /// <summary>Builds a sibling index path from the scene root down to the target, root included.</summary>
        public static string BuildSceneIndexPath(Transform target)
        {
            List<string> parts = new();
            Transform cursor = target;

            while (cursor != null)
            {
                parts.Add(cursor.GetSiblingIndex().ToString());
                cursor = cursor.parent;
            }

            parts.Reverse();
            return string.Join(SegmentSeparator.ToString(), parts);
        }

        /// <summary>Builds a name path from a root down to one of its descendants, root excluded.</summary>
        public static string BuildRelativeNamePath(Transform root, Transform target)
        {
            List<string> parts = new();
            Transform cursor = target;

            while (cursor != null && cursor != root)
            {
                parts.Add(cursor.name);
                cursor = cursor.parent;
            }

            if (cursor == null)
                return string.Empty;

            parts.Reverse();
            return string.Join(SegmentSeparator.ToString(), parts);
        }

        /// <summary>Builds a sibling index path from a root down to one of its descendants, root excluded.</summary>
        public static string BuildRelativeIndexPath(Transform root, Transform target)
        {
            List<string> parts = new();
            Transform cursor = target;

            while (cursor != null && cursor != root)
            {
                parts.Add(cursor.GetSiblingIndex().ToString());
                cursor = cursor.parent;
            }

            if (cursor == null)
                return string.Empty;

            parts.Reverse();
            return string.Join(SegmentSeparator.ToString(), parts);
        }

        /// <summary>Finds the object a scene path points at, or null.</summary>
        public static Transform ResolveInScene(Scene scene, string namePath, string indexPath)
        {
            string[] names = Split(namePath);
            if (names.Length == 0)
                return null;

            string[] indices = Split(indexPath);
            Transform root = ResolveRoot(scene.GetRootGameObjects(), names[0], GetIndex(indices, 0));
            if (root == null)
                return null;

            return Descend(root, names, indices, 1);
        }

        /// <summary>Finds the object a relative path points at, or the root itself when the path is empty.</summary>
        public static Transform ResolveFromRoot(Transform root, string namePath, string indexPath)
        {
            string[] names = Split(namePath);
            if (names.Length == 0)
                return root;

            return Descend(root, names, Split(indexPath), 0);
        }

        /// <summary>Finds a component by type name and its index among components of that type.</summary>
        public static Component ResolveComponent(Transform owner, string componentTypeName, int componentIndex)
        {
            int index = 0;
            foreach (Component candidate in owner.GetComponents<Component>())
            {
                if (candidate == null)
                    continue;

                if (candidate.GetType().FullName != componentTypeName)
                    continue;

                if (index == componentIndex)
                    return candidate;

                index++;
            }

            return null;
        }

        /// <summary>Returns the index of a component among the components of the same type on its GameObject.</summary>
        public static int FindComponentIndex(Component component)
        {
            int index = 0;
            foreach (Component candidate in component.gameObject.GetComponents<Component>())
            {
                if (candidate == null)
                    continue;

                if (candidate.GetType() != component.GetType())
                    continue;

                if (candidate == component)
                    return index;

                index++;
            }

            return 0;
        }

        private static Transform Descend(Transform start, string[] names, string[] indices, int firstSegment)
        {
            Transform current = start;

            for (int segment = firstSegment; segment < names.Length; segment++)
            {
                Transform next = FindChild(current, names[segment], GetIndex(indices, segment));
                if (next == null)
                    return null;

                current = next;
            }

            return current;
        }

        private static Transform FindChild(Transform parent, string name, int index)
        {
            if (index >= 0 && index < parent.childCount)
            {
                Transform candidate = parent.GetChild(index);
                if (candidate.name == name)
                    return candidate;
            }

            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
            }

            return null;
        }

        private static Transform ResolveRoot(GameObject[] roots, string name, int index)
        {
            if (index >= 0 && index < roots.Length && roots[index].name == name)
                return roots[index].transform;

            foreach (GameObject root in roots)
            {
                if (root.name == name)
                    return root.transform;
            }

            return null;
        }

        private static string[] Split(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Array.Empty<string>();

            return path.Split(SegmentSeparator);
        }

        private static int GetIndex(string[] indices, int segment)
        {
            if (segment >= indices.Length)
                return UnknownIndex;

            return int.TryParse(indices[segment], out int value)
                ? value
                : UnknownIndex;
        }
    }
}