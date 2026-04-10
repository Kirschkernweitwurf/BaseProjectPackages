using Systems.Services;
using Tracking;
using UnityEngine;

namespace Systems.PriorityTrackers
{
    /// <summary>
    /// Class to control the game's timescale based on priority requests.
    /// </summary>
    public class TimeScaleManager : GameServiceBehaviour
    {
        /// <summary>
        /// The default timescale to use when no requests are active.
        /// It is recommended to keep this at 1.
        /// </summary>
        private const float DefaultTime = 1f;

        public readonly PriorityTracker<float> TimeScaleTracker = new();

        protected override void Awake()
        {
            base.Awake();

            TimeScaleTracker.OnCurrentActiveItemChanged += OnCurrentActiveItemChanged;
        }

        private void OnCurrentActiveItemChanged(TrackedItem<float> trackedItem)
        {
            if (trackedItem == null)
            {
                ApplyTimeScale(DefaultTime);
                return;
            }

            ApplyTimeScale(trackedItem.Item);
        }

        /// <summary>
        /// Helper to apply the timescale in Unity.
        /// </summary>
        private static void ApplyTimeScale(float timeScale) => Time.timeScale = timeScale;
    }
}