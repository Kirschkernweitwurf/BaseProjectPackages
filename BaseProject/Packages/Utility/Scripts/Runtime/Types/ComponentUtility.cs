using UnityEngine;
using Utility.Logging;

namespace Utility.Types
{
    /// <summary>
    /// Utility class for working with Unity components.
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// Attempts to retrieve a component of identifier T from the given object or its parents.
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
                    CustomLogger.LogWarning($"TryGetComponentInParent failed: Object of identifier {obj.GetType()} " +
                                            "is not a GameObject or Component.", null);
                    return false;
            }
        }

        /// <summary>
        /// Helper for GameObject
        /// </summary>
        public static bool TryGetComponentInParent<T>(this GameObject go, out T component) where T : Component
        {
            component = go.GetComponentInParent<T>();
            return component != null;
        }

        /// <summary>
        /// Helper for Component
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component comp, out T component)
        {
            component = comp.GetComponentInParent<T>();
            return component != null;
        }
    }
}