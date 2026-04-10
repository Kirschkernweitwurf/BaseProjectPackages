using Systems.Services;
using Tracking;
using UnityEngine;

namespace Systems.PriorityTrackers
{
    /// <summary>
    /// Manages the cursor state based on priority requests, using a PriorityTracker instance.
    /// </summary>
    public class CursorManager : GameServiceBehaviour
    {
        [Tooltip("Default cursor settings to use when no requests are active.")]
        [SerializeField] private CursorRequest defaultCursorSettings = new();

        public readonly PriorityTracker<CursorRequest> CursorTracker = new();

        protected override void Awake()
        {
            base.Awake();

            CursorTracker.OnCurrentActiveItemChanged += HandleCursorChange;
            CursorTracker.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (CursorTracker != null)
                CursorTracker.OnCurrentActiveItemChanged -= HandleCursorChange;
        }

        /// <summary>
        /// Adds a cursor request with the specified priority on behalf of the caller.
        /// </summary>
        /// <param name="trackedItem">The cursor request to track.</param>
        private void HandleCursorChange(TrackedItem<CursorRequest> trackedItem)
        {
            if (trackedItem == null)
            {
                ApplyCursorState(defaultCursorSettings);
                return;
            }

            ApplyCursorState(trackedItem.Item);
        }

        /// <summary>
        /// Requests a cursor state change with the given priority.
        /// </summary>
        /// <param name="request">The cursor request.</param>
        private static void ApplyCursorState(CursorRequest request)
        {
            Cursor.visible = request.IsCursorVisible;
            Cursor.lockState = request.LockMode;
        }
    }
}