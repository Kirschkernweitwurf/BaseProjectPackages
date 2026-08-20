using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UtilityPackage
{
    /// <summary>
    /// Utility class for working with Unity components.
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// Attempts to retrieve a component of type <typeparamref name="T"/> from the given object or its parents.
        /// Mimics TryGetComponent, but searches the parent hierarchy.
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="target">The object to start the search from.</param>
        /// <param name="component">The found component, or null.</param>
        /// <returns>True if a component was found; otherwise, false.</returns>
        public static bool TryGetComponentInParent<T>(this Object target, out T component) where T : Component
        {
            component = null;

            if (target == null)
            {
                CustomLogger.LogWarning($"{nameof(TryGetComponentInParent)} failed: the target is null.", null);
                return false;
            }

            switch (target)
            {
                case GameObject targetObject:
                    return targetObject.TryGetComponentInParent(out component);

                case Component targetComponent:
                    return targetComponent.TryGetComponentInParent(out component);

                default:
                    CustomLogger.LogWarning($"{nameof(TryGetComponentInParent)} failed: an object of type "
                        + $"{target.GetType().Name} is neither a {nameof(GameObject)} nor a {nameof(Component)}.",
                        target);

                    return false;
            }
        }

        /// <summary>
        /// Attempts to retrieve a component of type <typeparamref name="T"/> from the given
        /// <see cref="GameObject"/> or its parents.
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="target">The GameObject to start the search from.</param>
        /// <param name="component">The found component, or null.</param>
        /// <returns>True if a component was found; otherwise, false.</returns>
        public static bool TryGetComponentInParent<T>(this GameObject target, out T component) where T : Component
        {
            component = target.GetComponentInParent<T>();
            return component != null;
        }

        /// <summary>
        /// Attempts to retrieve a component of type <typeparamref name="T"/> from the given
        /// <see cref="Component"/>'s object or its parents.
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <param name="target">The component whose object starts the search.</param>
        /// <param name="component">The found component, or null.</param>
        /// <returns>True if a component was found; otherwise, false.</returns>
        public static bool TryGetComponentInParent<T>(this Component target, out T component) where T : Component
        {
            component = target.GetComponentInParent<T>();
            return component != null;
        }
    }
}