using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Components.RendererTweens
{
    /// <summary>
    /// Tweens the color of a SpriteRenderer between two fixed values (startColor → targetColor).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteRendererColorTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The starting color to tween from.")]
        private Color startColor = Color.white;

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
                : startColor;

            Color to = isReversed
                ? startColor
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