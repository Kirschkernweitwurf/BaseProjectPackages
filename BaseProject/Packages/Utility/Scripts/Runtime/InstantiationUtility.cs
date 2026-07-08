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
        public static GameObject CleanInstantiate(GameObject prefab, Transform parent = null, bool dontDestroy = false)
        {
            if (!prefab)
                return null;

            GameObject instance = Object.Instantiate(prefab);
            if (dontDestroy)
                Object.DontDestroyOnLoad(instance);
            else if (parent)
                instance.transform.SetParent(parent);

            instance.name = instance.name.Replace("(Clone)", string.Empty);
            return instance;
        }
    }
}