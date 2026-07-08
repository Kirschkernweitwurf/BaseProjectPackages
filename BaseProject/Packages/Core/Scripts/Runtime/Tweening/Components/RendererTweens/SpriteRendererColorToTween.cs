using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.CorePackage.Tweening.Components.RendererTweens
{
    /// <summary>
    /// Tweens the color of a SpriteRenderer from the current color (captured at <c>Awake</c>)
    /// to a target color.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteRendererColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private SpriteRenderer _spriteRenderer;

#region Unity Callbacks
        protected override void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _spriteRenderer.color;

        protected override void ApplyValue(Color value) => _spriteRenderer.color = value;

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
                _spriteRenderer,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}