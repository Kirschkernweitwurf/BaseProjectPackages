using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Holds the components marked during the current play session.
    /// Deliberately in memory only and never persisted: a marked object cannot outlive a domain reload,
    /// so neither should the mark. This also avoids ids entirely, which sidesteps both the
    /// InstanceID to EntityId migration and the way play mode rewrites GlobalObjectIds.
    /// </summary>
    public static class PlayModeMarks
    {
        private static readonly List<Component> _marked = new();

        /// <summary>Components marked during the current play session.</summary>
        public static IReadOnlyList<Component> Components => _marked;

        /// <summary>Marks a component. Marking the same component twice is a no-op.</summary>
        public static void Add(Component component)
        {
            if (component == null)
                return;

            if (_marked.Contains(component))
                return;

            _marked.Add(component);
        }

        /// <summary>Removes a mark.</summary>
        public static void Remove(Component component) => _marked.Remove(component);

        /// <summary>Removes a mark by list index.</summary>
        public static void RemoveAt(int index)
        {
            if (index < 0
                || index >= _marked.Count)
                return;

            _marked.RemoveAt(index);
        }

        /// <summary>Returns true when the component is already marked.</summary>
        public static bool HasComponent(Component component) => component != null && _marked.Contains(component);

        /// <summary>Drops marks whose objects have been destroyed.</summary>
        public static void Prune()
        {
            for (int index = _marked.Count - 1; index >= 0; index--)
            {
                if (_marked[index] == null)
                    _marked.RemoveAt(index);
            }
        }

        /// <summary>Drops every mark.</summary>
        public static void Clear() => _marked.Clear();
    }
}
