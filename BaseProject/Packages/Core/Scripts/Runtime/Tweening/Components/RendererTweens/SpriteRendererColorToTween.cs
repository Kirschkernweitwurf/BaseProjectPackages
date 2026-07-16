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

        private SpriteRenderer _spriteRenderer;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => _spriteRenderer;

        protected override Color StartValue => DefaultValue;

        protected override Color LocalTargetValue => targetColor;

#region Unity Callbacks
        protected override void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _spriteRenderer.color;

        protected override void ApplyValue(Color value) => _spriteRenderer.color = value;
    }
}