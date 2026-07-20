using Base.AttributePackage;
using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the fill amount of a UI Image between two fixed values
    /// (startFillAmount → targetFillAmount).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageFillAmountTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting fill amount to tween from.")]
        private float startFillAmount;

        [SerializeField] [TweenValue] [Tooltip("The target fill amount to tween to.")]
        private float targetFillAmount = 1f;

        [GetComponent] [SerializeField] private Image image;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => image;

        protected override float LocalStartValue => startFillAmount;

        protected override float LocalTargetValue => targetFillAmount;

        protected override float GetCurrentValue() => image.fillAmount;

        protected override void ApplyValue(float value) => image.fillAmount = value;
    }
}