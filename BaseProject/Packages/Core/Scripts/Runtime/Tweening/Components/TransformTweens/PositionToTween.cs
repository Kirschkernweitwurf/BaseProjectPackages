using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the position of a Transform or RectTransform from the GameObject's start position
    /// (captured at <c>Awake</c>) to a target position.
    /// </summary>
    public sealed class PositionToTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private Vector3TweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target position to tween to.")]
        private Vector3 targetPosition;

        [SerializeField] [Tooltip("If true, tween the local position; otherwise, tween the global position.")]
        private bool useLocalPosition = true;

        protected override TweenValueProfileSo<Vector3> ProfileAsset => profile;

        protected override Object TweenTarget => transform;

        protected override Vector3 StartValue => DefaultValue;

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