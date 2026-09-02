using Base.AttributesPackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using Base.TweeningPackage.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Components.RendererTweens
{
    /// <summary>
    /// Tweens the color of a SpriteRenderer between two fixed values (startColor → targetColor).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteRendererColorTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting color to tween from.")]
        private Color startColor = Color.white;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        [GetComponent] [SerializeField] private SpriteRenderer spriteRenderer;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => spriteRenderer;

        protected override Color LocalStartValue => startColor;

        protected override Color LocalTargetValue => targetColor;

        protected override Color GetCurrentValue() => spriteRenderer.color;

        protected override void ApplyValue(Color value) => spriteRenderer.color = value;
    }
}