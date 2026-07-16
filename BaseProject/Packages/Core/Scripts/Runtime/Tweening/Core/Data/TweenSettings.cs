using System;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data
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

        /// <summary>Creates settings with the default values.</summary>
        public TweenSettings() { }

        /// <summary>Creates settings with the specified values.</summary>
        public TweenSettings(float duration, float delay, EEasingType easing)
        {
            Duration = duration;
            Delay = delay;
            Easing = easing;
        }

        /// <summary>
        /// Sets the duration of the tween to the specified value.
        /// </summary>
        public void SetDuration(float duration) => Duration = duration;

        /// <summary>
        /// Sets the delay of the tween to the specified value.
        /// </summary>
        public void SetDelay(float delay) => Delay = delay;

        /// <summary>
        /// Sets the easing of the tween to the specified value.
        /// </summary>
        public void SetEasing(EEasingType easing) => Easing = easing;

        /// <summary>
        /// Returns an independent copy of these settings. Used to keep runtime changes from
        /// leaking into shared assets.
        /// </summary>
        public TweenSettings Copy() => new(Duration, Delay, Easing);
    }
}