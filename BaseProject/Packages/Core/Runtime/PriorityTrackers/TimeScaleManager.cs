using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using UnityEngine;

namespace Base.CorePackage.PriorityTrackers
{
    /// <summary>
    /// Applies the highest-priority timescale request to <see cref="Time.timeScale"/>.
    /// Falls back to the default timescale while no request is active.
    /// </summary>
    public class TimeScaleManager : GameServiceBehaviour
    {
        private const float DefaultTimeScale = 1f;

        /// <summary>
        /// Resolves competing timescale requests by priority.
        /// </summary>
        public PriorityTracker<float> TimeScaleTracker { get; } = new();

#region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            TimeScaleTracker.OnCurrentActiveItemChanged += HandleTimeScaleChange;
            TimeScaleTracker.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            TimeScaleTracker.OnCurrentActiveItemChanged -= HandleTimeScaleChange;
        }
#endregion

        // A null item means nothing is requesting a timescale, so the default takes over.
        private static void HandleTimeScaleChange(TrackedItem<float> trackedItem)
        {
            if (trackedItem == null)
            {
                ApplyTimeScale(DefaultTimeScale);
                return;
            }

            ApplyTimeScale(trackedItem.Item);
        }

        private static void ApplyTimeScale(float timeScale) => Time.timeScale = timeScale;
    }
}