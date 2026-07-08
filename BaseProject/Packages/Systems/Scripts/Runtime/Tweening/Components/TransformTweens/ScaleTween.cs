using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the local scale of a Transform between two fixed values (startScale → targetScale).
    /// </summary>
    public sealed class ScaleTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The starting scale to tween from.")]
        private Vector3 startScale = Vector3.one;

        [SerializeField] [Tooltip("The target scale to tween to.")]
        private Vector3 targetScale = Vector3.one;

        protected override Vector3 GetCurrentValue() => transform.localScale;

        protected override void ApplyValue(Vector3 value) => transform.localScale = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            Vector3 from = isReversed
                ? targetScale
                : startScale;

            Vector3 to = isReversed
                ? startScale
                : targetScale;

            return new Tween<Vector3>(to,
                TweenSettings.Duration,
                ApplyValue,
                TweenLerpUtility.LerpVector3Unclamped,
                Easings.Get(TweenSettings.Easing),
                transform,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}