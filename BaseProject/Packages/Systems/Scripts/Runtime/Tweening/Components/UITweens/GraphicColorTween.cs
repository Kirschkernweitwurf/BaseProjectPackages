using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a UI Graphic.
    /// This can be used with any UI element that inherits from Graphic, such as Text, Image, etc.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class GraphicColorTween : TweenBehaviour<Color>
    {
        [SerializeField, Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Graphic _graphic;

        protected override void Awake()
        {
            _graphic = GetComponent<Graphic>();

            base.Awake();
        }

        protected override Color GetCurrentValue() => _graphic.color;

        protected override void ApplyValue(Color value) => _graphic.color = value;

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
                targetObj: _graphic,
                delay: TweenSettings.Delay,
                fromGetter: () => from
            );
        }
    }
}