using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;

namespace Systems.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the local scale of a Transform.
    /// </summary>
    public sealed class ScaleTween : TweenBehaviour<Vector3>
    {
        [SerializeField, Tooltip("The target scale to tween to.")]
        private Vector3 targetScale = Vector3.one;

        protected override Vector3 GetCurrentValue() => transform.localScale;

        protected override void ApplyValue(Vector3 value) => transform.localScale = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            Vector3 from = isReversed ? targetScale : DefaultValue;
            Vector3 to = isReversed ? DefaultValue : targetScale;

            return new Tween<Vector3>(
                to: to,
                duration: TweenSettings.Duration,
                setter: ApplyValue,
                lerpFunc: TweenLerpUtility.LerpVector3Unclamped,
                ease: Easings.Get(TweenSettings.Easing),
                targetObj: transform,
                delay: TweenSettings.Delay,
                fromGetter: () => from
            );
        }
    }
}