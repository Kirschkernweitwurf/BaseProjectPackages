using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// One object reference of a captured component, stored as a <see cref="GlobalObjectId"/> string.
    /// Instance ids are not stable across domain reloads, global object ids are.
    /// </summary>
    [Serializable]
    public class ComponentReferenceEntry
    {
        [SerializeField] private string propertyPath;
        [SerializeField] private string globalId;

        /// <summary>Serialized property path the reference belongs to.</summary>
        public string PropertyPath => propertyPath;

        /// <summary>Global object id of the referenced object, empty when the reference was null.</summary>
        public string GlobalId => globalId;

        /// <summary>Creates a reference entry.</summary>
        /// <param name="propertyPath">Serialized property path.</param>
        /// <param name="globalId">Global object id string, or empty for a null reference.</param>
        public ComponentReferenceEntry(string propertyPath, string globalId)
        {
            this.propertyPath = propertyPath;
            this.globalId = globalId;
        }

        /// <summary>Returns true when a target object was captured.</summary>
        public bool HasTarget() => !string.IsNullOrEmpty(globalId);

        /// <summary>
        /// Resolves the stored id back into an object. Scene objects only resolve while their scene
        /// is loaded.
        /// </summary>
        /// <returns>The referenced object, or null when it cannot be resolved.</returns>
        public Object Resolve()
        {
            if (!HasTarget())
                return null;

            if (!GlobalObjectId.TryParse(globalId, out GlobalObjectId parsed))
                return null;

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsed);
        }
    }
}