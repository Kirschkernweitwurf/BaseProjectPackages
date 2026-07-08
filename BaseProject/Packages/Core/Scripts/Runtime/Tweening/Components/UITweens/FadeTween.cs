using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using UnityEngine;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a CanvasGroup between two fixed values (startAlpha → targetAlpha).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FadeTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The starting alpha value.")]
        private float startAlpha;

        [SerializeField] [Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        private CanvasGroup _canvasGroup;

#region Unity Callbacks
        protected override void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            base.Awake();
        }
#endregion

        protected override float GetCurrentValue() => _canvasGroup.alpha;

        protected override void ApplyValue(float value) => _canvasGroup.alpha = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            float from = isReversed
                ? targetAlpha
                : startAlpha;

            float to = isReversed
                ? startAlpha
                : targetAlpha;

            return new Tween<float>(to,
                TweenSettings.Duration,
                ApplyValue,
                TweenLerpUtility.LerpFloatUnclamped,
                Easings.Get(TweenSettings.Easing),
                _canvasGroup,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}