using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SystemsCorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the fill amount of a UI Image between two fixed values (startFillAmount → targetFillAmount).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageFillAmountTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The starting fill amount to tween from.")]
        private float startFillAmount;

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
                : startFillAmount;

            float to = isReversed
                ? startFillAmount
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