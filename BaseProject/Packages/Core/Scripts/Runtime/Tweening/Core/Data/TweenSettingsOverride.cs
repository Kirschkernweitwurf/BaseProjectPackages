using System;
using Base.AttributePackage;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Optional per field overrides applied on top of resolved <see cref="TweenSettings"/>.
    /// Every field is opt in, so an untouched override changes nothing.
    /// </summary>
    [Serializable]
    public class TweenSettingsOverride
    {
        [Tooltip("If true, the duration below replaces the resolved duration.")]
        [SerializeField] private bool overrideDuration;

        [ShowIf(nameof(overrideDuration))] [Min(0f)] [SerializeField] private float duration = 0.5f;

        [Tooltip("If true, the delay below replaces the resolved delay.")]
        [SerializeField] private bool overrideDelay;

        [ShowIf(nameof(overrideDelay))] [Min(0f)] [SerializeField] private float delay;

        [Tooltip("If true, the easing below replaces the resolved easing.")]
        [SerializeField] private bool overrideEasing;

        [ShowIf(nameof(overrideEasing))] [SerializeField] private EEasingType easing;

        /// <summary>True if at least one field is overridden.</summary>
        public bool HasAnyOverride => overrideDuration || overrideDelay || overrideEasing;

        /// <summary>
        /// Returns an independent copy of <paramref name="source"/> with the enabled overrides
        /// applied. A null source falls back to fresh default settings.
        /// </summary>
        public TweenSettings Apply(TweenSettings source)
        {
            TweenSettings result = source?.Copy() ?? new TweenSettings();

            if (overrideDuration)
                result.SetDuration(duration);

            if (overrideDelay)
                result.SetDelay(delay);

            if (overrideEasing)
                result.SetEasing(easing);

            return result;
        }
    }
}