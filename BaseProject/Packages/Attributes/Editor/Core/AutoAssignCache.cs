using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Caches the results of the project-wide and scene-wide searches the auto-getters run. The
    /// inspector redraws constantly, so an uncached asset database query or scene walk would run dozens
    /// of times a second on a single selected object.
    /// </summary>
    /// <remarks>
    /// Misses are cached too, otherwise a field that legitimately has nothing to find would search on
    /// every repaint forever. Asset results are dropped when the project changes, scene results when the
    /// hierarchy does, and both on a domain reload.
    /// </remarks>
    internal static class AutoAssignCache
    {
        private static readonly Dictionary<Type, Object> Assets = new();

        private static readonly Dictionary<Type, Object> SceneObjects = new();

        static AutoAssignCache()
        {
            EditorApplication.projectChanged += Assets.Clear;
            EditorApplication.hierarchyChanged += SceneObjects.Clear;
            AssemblyReloadEvents.beforeAssemblyReload += DropAll;
        }

        /// <summary>Returns the cached asset for the given type, running the search on first use.</summary>
        /// <param name="type">The type being searched for.</param>
        /// <param name="search">The search to run on a cache miss.</param>
        /// <returns>The found asset, or null.</returns>
        internal static Object GetAsset(Type type, Func<Type, Object> search) => Get(Assets, type, search);

        /// <summary>Returns the cached scene object for the given type, running the search on first use.</summary>
        /// <param name="type">The type being searched for.</param>
        /// <param name="search">The search to run on a cache miss.</param>
        /// <returns>The found object, or null.</returns>
        internal static Object GetSceneObject(Type type, Func<Type, Object> search) => Get(SceneObjects, type, search);

        // Presence in the dictionary means the search already ran, whatever it found. A stored null is a
        // cached miss and stays one until the project or the hierarchy invalidates it, which is also
        // when a deleted result would have needed re-searching anyway.
        private static void DropAll()
        {
            Assets.Clear();
            SceneObjects.Clear();
        }

        private static Object Get(Dictionary<Type, Object> cache, Type type, Func<Type, Object> search)
        {
            if (cache.TryGetValue(type, out Object cached))
                return cached == null
                    ? null
                    : cached;

            Object found = search(type);
            cache[type] = found;
            return found;
        }
    }
}