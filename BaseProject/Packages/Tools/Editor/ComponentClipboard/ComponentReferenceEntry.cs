using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// One object reference of a captured component, stored as a <see cref="GlobalObjectId"/> string.
    /// Instance ids are not stable across domain reloads, global object ids are.
    /// </summary>
    [Serializable]
    internal class ComponentReferenceEntry
    {
        [SerializeField] private string propertyPath;
        [SerializeField] private string globalId;

        /// <summary>Serialized property path the reference belongs to.</summary>
        internal string PropertyPath => propertyPath;

        /// <summary>Creates a reference entry.</summary>
        /// <param name="propertyPath">Serialized property path.</param>
        /// <param name="globalId">Global object id string, or empty for a null reference.</param>
        public ComponentReferenceEntry(string propertyPath, string globalId)
        {
            this.propertyPath = propertyPath;
            this.globalId = globalId;
        }

        /// <summary>Returns true when a target object was captured.</summary>
        internal bool HasTarget() => !string.IsNullOrEmpty(globalId);

        /// <summary>
        /// Resolves the stored id back into an object. Scene objects only resolve while their scene
        /// is loaded.
        /// </summary>
        /// <returns>The referenced object, or null when it cannot be resolved.</returns>
        internal Object Resolve()
        {
            if (!HasTarget())
                return null;

            if (!GlobalObjectId.TryParse(globalId, out GlobalObjectId parsed))
                return null;

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsed);
        }
    }
}