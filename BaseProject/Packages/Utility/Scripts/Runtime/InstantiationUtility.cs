using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Helper methods for instantiating prefabs with clean names.
    /// </summary>
    public static class InstantiationUtility
    {
        /// <summary>
        /// Instantiates a prefab, removes "(Clone)" from its name, and optionally parents it
        /// or marks it to not be destroyed on load.
        /// </summary>
        /// <remarks>
        /// When <paramref name="dontDestroy"/> is <c>true</c>, <paramref name="parent"/> is ignored, since
        /// <see cref="Object.DontDestroyOnLoad"/> only works on root objects.
        /// </remarks>
        public static GameObject CleanInstantiate(GameObject prefab, Transform parent = null, bool dontDestroy = false)
        {
            if (!prefab)
                return null;

            GameObject instance = dontDestroy
                ? Object.Instantiate(prefab)
                : Object.Instantiate(prefab, parent);

            if (dontDestroy)
                Object.DontDestroyOnLoad(instance);

            instance.name = prefab.name;
            return instance;
        }
    }
}