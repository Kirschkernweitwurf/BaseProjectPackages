using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the fill amount of a UI Image from the current fill amount (captured at <c>Awake</c>)
    /// to a target fill amount.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageFillAmountToTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The target fill amount to tween to.")]
        private float targetFillAmount = 1f;

        private Image _image;

#region Unity Callbacks
        protected override void Awake()
        {
            _image = GetComponent<Image>();

            base.Awake();
        }
#endregion

        protected override float GetCurrentValue() => _image.fillAmount;

        protected override void ApplyValue(float value) => _image.fillAmount = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            float from = isReversed
                ? targetFillAmount
                : DefaultValue;

            float to = isReversed
                ? DefaultValue
                : targetFillAmount;

            return new Tween<float>(to,
                TweenSettings.Duration,
                ApplyValue,
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(TweenSettings.Easing),
                _image,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}