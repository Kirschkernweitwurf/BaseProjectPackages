using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Upward hierarchy search for <see cref="GetComponentInParentAttribute"/>. Starts at the parent
    /// and walks strictly upward, so the own GameObject is never used. Shared by the inspector handler
    /// and the batch assigner, so both resolve references identically.
    /// </summary>
    public static class ParentComponentSearch
    {
        /// <summary>
        /// Finds the first matching object on an ancestor. With a name only ancestors of that name are
        /// considered. Returns the Transform or GameObject itself when the field type asks for one.
        /// </summary>
        public static Object FindInParents(Transform start, Type type, string name, bool includeInactive)
        {
            for (Transform current = start.parent; current != null; current = current.parent)
            {
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                if (!string.IsNullOrEmpty(name) && current.name != name)
                    continue;

                Object match = Match(current, type);
                if (match != null)
                    return match;
            }

            return null;
        }

        private static Object Match(Transform current, Type type)
        {
            if (type == typeof(Transform))
                return current;

            if (type == typeof(GameObject))
                return current.gameObject;

            return current.GetComponent(type);
        }
    }
}