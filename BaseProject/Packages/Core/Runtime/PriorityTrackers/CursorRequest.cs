using System;
using UnityEngine;

namespace Base.CorePackage.PriorityTrackers
{
    /// <summary>
    /// Immutable cursor state that can be requested through the <see cref="CursorManager"/>.
    /// </summary>
    [Serializable]
    public class CursorRequest
    {
        /// <summary>
        /// Whether the hardware cursor is visible.
        /// </summary>
        [field: SerializeField] public bool IsCursorVisible { get; private set; }

        /// <summary>
        /// How the cursor is confined to the game window.
        /// </summary>
        [field: SerializeField] public CursorLockMode LockMode { get; private set; }

        /// <summary>
        /// Creates a cursor request.
        /// </summary>
        /// <param name="isCursorVisible">Whether the cursor is visible.</param>
        /// <param name="lockMode">How the cursor is confined to the game window.</param>
        public CursorRequest(bool isCursorVisible = true, CursorLockMode lockMode = CursorLockMode.None)
        {
            IsCursorVisible = isCursorVisible;
            LockMode = lockMode;
        }
    }
}