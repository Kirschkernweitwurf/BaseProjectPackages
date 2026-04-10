using System;
using UnityEngine;

namespace Systems.Tweening.Core.Data.Parameters
{
    /// <summary>
    /// Serializable data describing basic tween parameters.
    /// </summary>
    [Serializable]
    public struct TweenData
    {
        [field: Tooltip("Tween duration in seconds.")]
        [field: SerializeField] public float Duration { get; private set; }

        [field: Tooltip("Easing function used for interpolation.")]
        [field: SerializeField] public EEasingType Easing { get; private set; }

        [field: Tooltip("Optional delay before the tween starts (seconds).")]
        [field: SerializeField] public float Delay { get; private set; }

        public TweenData(float duration, EEasingType easing, float delay = 0f)
        {
            Duration = duration;
            Easing = easing;
            Delay = delay;
        }

        /// <summary>
        /// Returns a copy of this <see cref="TweenData"/> with the specified duration.
        /// </summary>
        public TweenData WithDuration(float duration) => new(duration, Easing, Delay);

        /// <summary>
        /// Returns a copy of this <see cref="TweenData"/> with the specified easing type.
        /// </summary>
        public TweenData WithEasing(EEasingType easing) => new(Duration, easing, Delay);

        /// <summary>
        /// Returns a copy of this <see cref="TweenData"/> with the specified delay.
        /// </summary>
        public TweenData WithDelay(float delay) => new(Duration, Easing, delay);
    }
}