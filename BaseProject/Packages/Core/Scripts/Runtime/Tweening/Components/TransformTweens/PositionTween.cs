using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the position of a Transform or RectTransform between two fixed values
    /// (initialPosition → targetPosition).
    /// </summary>
    public sealed class PositionTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private Vector3TweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting position used as the tween's 'from' value.")]
        private Vector3 initialPosition;

        [SerializeField] [TweenValue] [Tooltip("The target position to tween to.")]
        private Vector3 targetPosition;

        [SerializeField] [Tooltip("If true, tween the local position; otherwise, tween the global position.")]
        private bool useLocalPosition = true;

        protected override TweenValueProfileSo<Vector3> ProfileAsset => profile;

        protected override Object TweenTarget => transform;

        protected override Vector3 LocalStartValue => initialPosition;

        protected override Vector3 LocalTargetValue => targetPosition;

        protected override Vector3 GetCurrentValue() => useLocalPosition
            ? transform.localPosition
            : transform.position;

        protected override void ApplyValue(Vector3 value)
        {
            if (useLocalPosition)
                transform.localPosition = value;
            else
                transform.position = value;
        }
    }
}