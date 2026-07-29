using System;
using Base.UtilityPackage.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Helper methods for instantiating prefabs with clean names.
    /// </summary>
    public static class InstantiationUtility
    {
        /// <summary>
        /// The suffix Unity appends to the name of every instantiated object.
        /// </summary>
        public const string CloneSuffix = "(Clone)";

        /// <summary>
        /// Instantiates a prefab, removes "(Clone)" from its name, and optionally parents it
        /// or marks it to not be destroyed on load.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="parent">Optional parent for the new instance.</param>
        /// <param name="dontDestroy">Whether the instance survives scene loads.</param>
        /// <returns>The new instance, or null if the prefab was missing.</returns>
        /// <remarks>
        /// When <paramref name="dontDestroy"/> is <c>true</c>, <paramref name="parent"/> is ignored, since
        /// <see cref="Object.DontDestroyOnLoad"/> only works on root objects.
        /// </remarks>
        public static GameObject CleanInstantiate(GameObject prefab, Transform parent = null, bool dontDestroy = false)
        {
            if (prefab == null)
            {
                CustomLogger.LogWarning($"{nameof(CleanInstantiate)} was called without a {nameof(prefab)}.", null);
                return null;
            }

            GameObject instance = dontDestroy
                ? Object.Instantiate(prefab)
                : Object.Instantiate(prefab, parent);

            if (dontDestroy)
                Object.DontDestroyOnLoad(instance);

            instance.name = prefab.name;
            return instance;
        }

        /// <summary>
        /// Instantiates any object type and removes "(Clone)" from its name. Use this for components and
        /// ScriptableObjects, where the DontDestroyOnLoad handling of the GameObject overload does not apply.
        /// </summary>
        /// <typeparam name="T">The object type to spawn.</typeparam>
        /// <param name="prefab">The object to spawn.</param>
        /// <param name="parent">Optional parent for the new instance.</param>
        /// <returns>The new instance, or null if the prefab was missing.</returns>
        public static T CleanInstantiate<T>(T prefab, Transform parent = null) where T : Object
        {
            if (prefab == null)
            {
                CustomLogger.LogWarning($"{nameof(CleanInstantiate)} was called without a {nameof(prefab)}.", null);
                return null;
            }

            T instance = Object.Instantiate(prefab, parent);
            instance.name = prefab.name;
            return instance;
        }

        /// <summary>
        /// Removes every trailing "(Clone)" from a name, along with the whitespace in front of it.
        /// Instantiating an instance appends the suffix again, so a name can carry more than one.
        /// </summary>
        /// <param name="name">The name to clean.</param>
        /// <returns>The name without the suffix.</returns>
        public static string StripCloneSuffix(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            while (name.EndsWith(CloneSuffix, StringComparison.Ordinal))
                name = name[..^CloneSuffix.Length].TrimEnd();

            return name;
        }
    }
}