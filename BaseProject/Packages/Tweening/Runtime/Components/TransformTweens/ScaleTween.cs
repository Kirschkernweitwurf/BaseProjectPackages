using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using Base.TweeningPackage.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Components.TransformTweens
{
    /// <summary>
    /// Tweens the local scale of a Transform between two fixed values (startScale → targetScale).
    /// </summary>
    public sealed class ScaleTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private Vector3TweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting scale to tween from.")]
        private Vector3 startScale = Vector3.one;

        [SerializeField] [TweenValue] [Tooltip("The target scale to tween to.")]
        private Vector3 targetScale = Vector3.one;

        protected override TweenValueProfileSo<Vector3> ProfileAsset => profile;

        protected override Object TweenTarget => transform;

        protected override Vector3 LocalStartValue => startScale;

        protected override Vector3 LocalTargetValue => targetScale;

        protected override Vector3 GetCurrentValue() => transform.localScale;

        protected override void ApplyValue(Vector3 value) => transform.localScale = value;
    }
}