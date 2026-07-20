using Base.AttributePackage;
using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a CanvasGroup between two fixed values (startAlpha → targetAlpha).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FadeTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting alpha value.")]
        private float startAlpha;

        [SerializeField] [TweenValue] [Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        [GetComponent] [SerializeField] private CanvasGroup canvasGroup;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => canvasGroup;

        protected override float LocalStartValue => startAlpha;

        protected override float LocalTargetValue => targetAlpha;

        protected override float GetCurrentValue() => canvasGroup.alpha;

        protected override void ApplyValue(float value) => canvasGroup.alpha = value;
    }
}