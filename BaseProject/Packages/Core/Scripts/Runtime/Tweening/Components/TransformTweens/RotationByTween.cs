using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.TransformTweens
{
    /// <summary>
    /// Tweens the rotation of a Transform by a delta in Euler degrees, relative to the
    /// rotation at the moment the tween is created.
    /// </summary>
    /// <remarks>
    /// A profile's target value is read as the delta here, its start value is unused.
    /// </remarks>
    public sealed class RotationByTween : TweenBehaviour<Vector3>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private Vector3TweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("Rotation delta in Euler degrees.")]
        private Vector3 deltaEulerAngles;

        [SerializeField] [Tooltip("If true, tween the local rotation; otherwise, tween the global rotation.")]
        private bool useLocalRotation = true;

        protected override TweenValueProfileSo<Vector3> ProfileAsset => profile;

        protected override Object TweenTarget => transform;

        protected override Vector3 StartValue => GetCurrentValue();

        protected override Vector3 TargetValue => GetCurrentValue() + Delta;

        private Vector3 Delta => Profile != null
            ? Profile.TargetValue
            : deltaEulerAngles;

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