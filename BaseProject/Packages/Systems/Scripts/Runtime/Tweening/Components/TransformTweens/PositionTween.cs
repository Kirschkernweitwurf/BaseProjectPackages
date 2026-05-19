using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the position of a Transform or RectTransform between two fixed values
    /// (initialPosition → targetPosition).
    /// </summary>
    public sealed class PositionTween : TweenBehaviour<Vector3>
    {
        [SerializeField, Tooltip("The starting position used as the tween's 'from' value.")]
        private Vector3 initialPosition;

        [SerializeField, Tooltip("The target position to tween to.")]
        private Vector3 targetPosition;

        [SerializeField, Tooltip("If true, tween the local position; otherwise, tween the global position.")]
        private bool useLocalPosition = true;

        protected override Vector3 GetCurrentValue() => useLocalPosition ? transform.localPosition : transform.position;

        protected override void ApplyValue(Vector3 value)
        {
            if (useLocalPosition)
                transform.localPosition = value;
            else
                transform.position = value;
        }

        protected override TweenBase CreateTween(bool isReversed)
        {
            Vector3 from = isReversed ? targetPosition : initialPosition;
            Vector3 to = isReversed ? initialPosition : targetPosition;

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
