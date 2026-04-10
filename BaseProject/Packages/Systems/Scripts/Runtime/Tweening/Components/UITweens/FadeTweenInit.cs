using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;

namespace Systems.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class FadeTweenInit : TweenBehaviour<float>
    {
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
            float from = isReversed ? targetAlpha : DefaultValue;
            float to = isReversed ? DefaultValue : targetAlpha;

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