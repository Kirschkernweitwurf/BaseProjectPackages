using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;

namespace Systems.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FadeTween : TweenBehaviour<float>
    {
        [SerializeField, Tooltip("The starting alpha value.")]
        private float startAlpha;

        [SerializeField, Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        private CanvasGroup _canvasGroup;

        protected override void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            base.Awake();
        }

        protected override float GetCurrentValue() => _canvasGroup.alpha;

        protected override void ApplyValue(float value) => _canvasGroup.alpha = value;

        protected override TweenBase CreateTween(bool isReversed)
        {
            float from = isReversed ? targetAlpha : startAlpha;
            float to = isReversed ? startAlpha : targetAlpha;

            return new Tween<float>(
                to: to,
                duration: TweenSettings.Duration,
                setter: ApplyValue,
                lerpFunc: TweenLerpUtility.LerpFloatUnclamped,
                ease: Easings.Get(TweenSettings.Easing),
                targetObj: _canvasGroup,
                delay: TweenSettings.Delay,
                fromGetter: () => from
            );
        }
    }
}