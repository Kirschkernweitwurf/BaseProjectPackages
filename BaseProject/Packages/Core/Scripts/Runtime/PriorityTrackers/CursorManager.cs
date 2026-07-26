using Base.CorePackage.Services;
using Base.CorePackage.Tracking;
using UnityEngine;

namespace Base.CorePackage.PriorityTrackers
{
    /// <summary>
    /// Applies the highest-priority <see cref="CursorRequest"/> to the hardware cursor.
    /// Falls back to the serialized default while no request is active.
    /// </summary>
    public class CursorManager : GameServiceBehaviour
    {
        [Tooltip("Cursor settings used when no request is active.")]
        [SerializeField] private CursorRequest defaultCursorSettings = new();

        /// <summary>
        /// Resolves competing cursor requests by priority.
        /// </summary>
        public PriorityTracker<CursorRequest> CursorTracker { get; } = new();

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            CursorTracker.OnCurrentActiveItemChanged += HandleCursorChange;
            CursorTracker.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            CursorTracker.OnCurrentActiveItemChanged -= HandleCursorChange;
        }
#endregion

        private static void ApplyCursorState(CursorRequest request)
        {
            Cursor.visible = request.IsCursorVisible;
            Cursor.lockState = request.LockMode;
        }

        // A null item means nothing is requesting a cursor state, so the serialized default takes over.
        private void HandleCursorChange(TrackedItem<CursorRequest> trackedItem)
        {
            if (trackedItem == null)
            {
                ApplyCursorState(defaultCursorSettings);
                return;
            }

            ApplyCursorState(trackedItem.Item);
        }
    }
}