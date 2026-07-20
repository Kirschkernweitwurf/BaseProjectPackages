using Base.AttributePackage;
using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.RendererTweens
{
    /// <summary>
    /// Tweens the color of a SpriteRenderer from the current color (captured at <c>Awake</c>)
    /// to a target color.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteRendererColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        [GetComponent] [SerializeField] private SpriteRenderer spriteRenderer;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => spriteRenderer;

        protected override Color StartValue => DefaultValue;

        protected override Color LocalTargetValue => targetColor;

        protected override Color GetCurrentValue() => spriteRenderer.color;

        protected override void ApplyValue(Color value) => spriteRenderer.color = value;
    }
}