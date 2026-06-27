using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SystemsCorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a UI Graphic from the Graphic's start color (captured at <c>Awake</c>) to a target color.
    /// This can be used with any UI element that inherits from Graphic, such as Text, Image, etc.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class GraphicColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Graphic _graphic;

#region Unity Callbacks
        protected override void Awake()
        {
            _graphic = GetComponent<Graphic>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _graphic.color;

        protected override void ApplyValue(Color value) => _graphic.color = value;

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
                _graphic,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}