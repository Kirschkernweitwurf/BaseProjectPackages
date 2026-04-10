using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a UI Image.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageColorTween : TweenBehaviour<Color>
    {
        [SerializeField, Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Image _image;

        protected override void Awake()
        {
            _image = GetComponent<Image>();

            base.Awake();
        }

        protected override Color GetCurrentValue() => _image.color;

        protected override void ApplyValue(Color value) => _image.color = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            Color from = isReversed ? targetColor : DefaultValue;
            Color to = isReversed ? DefaultValue : targetColor;

            return new Tween<Color>(
                to: to,
                duration: TweenSettings.Duration,
                setter: ApplyValue,
                lerpFunc: TweenLerpUtility.LerpColorUnclamped,
                ease: Easings.Get(TweenSettings.Easing),
                targetObj: _image,
                delay: TweenSettings.Delay,
                fromGetter: () => from
            );
        }
    }
}