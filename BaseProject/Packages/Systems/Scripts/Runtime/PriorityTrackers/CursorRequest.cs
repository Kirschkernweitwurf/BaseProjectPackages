using System;
using UnityEngine;

namespace Systems.PriorityTrackers
{
    /// <summary>
    /// Represents settings for the cursor, including visibility and lock mode.
    /// </summary>
    [Serializable]
    public class CursorRequest
    {
        [field: SerializeField] public bool IsCursorVisible { get; private set; }
        [field: SerializeField] public CursorLockMode LockMode { get; private set; }

        public CursorRequest(bool visible = true, CursorLockMode lockMode = CursorLockMode.None)
        {
            IsCursorVisible = visible;
            LockMode = lockMode;
        }
    }
}