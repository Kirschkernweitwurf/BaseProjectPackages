using System;
using UnityEngine;

namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Settings for individual tweens.
    /// </summary>
    [Serializable]
    public class TweenSettings
    {
        [field: Tooltip("Duration of the tween in seconds.")]
        [field: SerializeField] public float Duration { get; private set; } = 0.5f;

        [field: Tooltip("Delay before the tween starts.")]
        [field: SerializeField] public float Delay { get; private set; }

        [field: Tooltip("Easing function to use for the tween.")]
        [field: SerializeField] public EEasingType Easing { get; private set; }

        /// <summary>
        /// Sets the duration of the tween to the specified value.
        /// </summary>
        public void SetDuration(float duration) => Duration = duration;

        /// <summary>
        /// Sets the delay of the tween to the specified value.
        /// </summary>
        public void SetDelay(float delay) => Delay = delay;
    }
}