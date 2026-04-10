using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;

namespace Systems.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the rotation of a Transform.
    /// </summary>
    public sealed class RotationTween : TweenBehaviour<Vector3>
    {
        [SerializeField, Tooltip("Rotation delta in Euler degrees.")]
        private Vector3 deltaEulerAngles;

        [SerializeField, Tooltip("If true, tween the local rotation.")]
        private bool useLocalRotation = true;

        protected override Vector3 GetCurrentValue()
        {
            return useLocalRotation
                ? transform.localEulerAngles
                : transform.eulerAngles;
        }

        protected override void ApplyValue(Vector3 euler)
        {
            Quaternion q = Quaternion.Euler(euler);
            if (useLocalRotation)
                transform.localRotation = q;
            else
                transform.rotation = q;
        }

        protected override TweenBase CreateTween(bool isReversed)
        {
            Vector3 start = GetCurrentValue();
            Vector3 end = start + deltaEulerAngles;

            Vector3 from = isReversed ? end : start;
            Vector3 to = isReversed ? start : end;

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