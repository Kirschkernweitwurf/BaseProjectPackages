using System;
using UnityEngine;

namespace Base.CorePackage.GameActions
{
    /// <summary>
    /// Serializable handle to a single game action, chosen in the inspector by its stable
    /// id and resolved to a concrete <see cref="Type"/> through <see cref="GameActionRegistry"/>.
    /// </summary>
    [Serializable]
    public struct GameActionReference
    {
        [SerializeField] private string id;

        /// <summary>
        /// The stable id of the referenced action.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// True when an id has been assigned.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(id);

        /// <summary>
        /// Resolves the referenced action to its concrete type.
        /// </summary>
        /// <param name="type">The resolved action type when found.</param>
        /// <returns><c>true</c> when the id maps to a registered action.</returns>
        public bool TryResolve(out Type type)
        {
            if (GameActionRegistry.TryGet(id, out GameActionInfo info))
            {
                type = info.Type;
                return true;
            }

            type = null;
            return false;
        }
    }
}