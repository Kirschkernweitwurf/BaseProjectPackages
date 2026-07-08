using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the rotation of a Transform between two fixed Euler values (startEulerAngles → targetEulerAngles).
    /// </summary>
    public sealed class RotationTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The starting rotation in Euler degrees.")]
        private Vector3 startEulerAngles;

        [SerializeField] [Tooltip("The target rotation in Euler degrees.")]
        private Vector3 targetEulerAngles;

        [SerializeField] [Tooltip("If true, tween the local rotation; otherwise, tween the global rotation.")]
        private bool useLocalRotation = true;

        protected override Vector3 GetCurrentValue() => useLocalRotation
            ? transform.localEulerAngles
            : transform.eulerAngles;

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
            Vector3 from = isReversed
                ? targetEulerAngles
                : startEulerAngles;

            Vector3 to = isReversed
                ? startEulerAngles
                : targetEulerAngles;

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