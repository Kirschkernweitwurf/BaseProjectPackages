using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Keeps one nested editor per referenced asset alive across repaints. Creating an editor every
    /// frame leaks native objects and resets foldout state, so instances are cached by target and
    /// released when the target is gone or the domain reloads.
    /// </summary>
    internal static class EmbeddedEditorCache
    {
        private static readonly Dictionary<int, UnityEditor.Editor> Editors = new();

        static EmbeddedEditorCache() => AssemblyReloadEvents.beforeAssemblyReload += Clear;

        /// <summary>
        /// Returns the cached editor for the given target, creating it on first use. Returns null when
        /// the target is missing.
        /// </summary>
        /// <param name="target">The asset to draw an inline inspector for.</param>
        /// <returns>The nested editor, or null.</returns>
        public static UnityEditor.Editor Get(Object target)
        {
            if (target == null)
                return null;

            int id = target.GetInstanceID();

            if (Editors.TryGetValue(id, out UnityEditor.Editor cached) && cached != null && cached.target == target)
                return cached;

            Release(id);

            UnityEditor.Editor created = UnityEditor.Editor.CreateEditor(target);
            Editors[id] = created;
            return created;
        }

        /// <summary>Destroys every cached editor.</summary>
        public static void Clear()
        {
            foreach (UnityEditor.Editor editor in Editors.Values)
            {
                if (editor != null)
                    Object.DestroyImmediate(editor);
            }

            Editors.Clear();
        }

        private static void Release(int id)
        {
            if (!Editors.TryGetValue(id, out UnityEditor.Editor existing))
                return;

            if (existing != null)
                Object.DestroyImmediate(existing);

            Editors.Remove(id);
        }
    }
}