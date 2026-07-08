using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a UI Image from the Image's start color (captured at <c>Awake</c>) to a target color.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Image _image;

#region Unity Callbacks
        protected override void Awake()
        {
            _image = GetComponent<Image>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _image.color;

        protected override void ApplyValue(Color value) => _image.color = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            Color from = isReversed
                ? targetColor
                : DefaultValue;

            Color to = isReversed
                ? DefaultValue
                : targetColor;

            return new Tween<Color>(to,
                TweenSettings.Duration,
                ApplyValue,
                TweenLerpUtility.LerpColorUnclamped,
                Easings.Get(TweenSettings.Easing),
                _image,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}