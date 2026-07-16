using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the rotation of a Transform between two fixed Euler values (startEulerAngles → targetEulerAngles).
    /// </summary>
    public sealed class RotationTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private Vector3TweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting rotation in Euler degrees.")]
        private Vector3 startEulerAngles;

        [SerializeField] [TweenValue] [Tooltip("The target rotation in Euler degrees.")]
        private Vector3 targetEulerAngles;

        [SerializeField] [Tooltip("If true, tween the local rotation; otherwise, tween the global rotation.")]
        private bool useLocalRotation = true;

        protected override TweenValueProfileSo<Vector3> ProfileAsset => profile;

        protected override Object TweenTarget => transform;

        protected override Vector3 LocalStartValue => startEulerAngles;

        protected override Vector3 LocalTargetValue => targetEulerAngles;

        protected override Vector3 GetCurrentValue() => useLocalRotation
            ? transform.localEulerAngles
            : transform.eulerAngles;

        protected override void ApplyValue(Vector3 euler)
        {
            Quaternion rotation = Quaternion.Euler(euler);

            if (useLocalRotation)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }
    }
}