using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UtilityPackage.Types
{
    /// <summary>
    /// Utility class for working with Unity components.
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// Attempts to retrieve a component of type T from the given object or its parents.
        /// Mimics TryGetComponent, but searches the parent hierarchy.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Object obj, out T component) where T : Component
        {
            component = null;

            switch (obj)
            {
                case GameObject go:
                    return go.TryGetComponentInParent(out component);

                case Component comp:
                    return comp.TryGetComponentInParent(out component);

                default:
                    CustomLogger.LogWarning($"{nameof(TryGetComponentInParent)} failed: Object of type "
                        + $"{obj.GetType().Name} is not a {nameof(GameObject)} or {nameof(Component)}.", null);

                    return false;
            }
        }

        /// <summary>
        /// Attempts to retrieve a component of type T from the given <see cref="GameObject"/> or its parents.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponentInParent<T>();
            return component != null;
        }

        /// <summary>
        /// Attempts to retrieve a component of type T from the given <see cref="Component"/>'s object or its parents.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component comp, out T component)
        {
            component = comp.GetComponentInParent<T>();
            return component != null;
        }
    }
}