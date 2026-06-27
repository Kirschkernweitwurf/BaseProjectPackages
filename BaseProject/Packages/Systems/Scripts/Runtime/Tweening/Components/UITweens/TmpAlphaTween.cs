using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using TMPro;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a TextMeshPro text between two fixed values (startAlpha → targetAlpha).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpAlphaTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The starting alpha value.")]
        private float startAlpha;

        [SerializeField] [Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        private TMP_Text _text;

#region Unity Callbacks
        protected override void Awake()
        {
            _text = GetComponent<TMP_Text>();

            base.Awake();
        }
#endregion

        protected override float GetCurrentValue() => _text.alpha;

        protected override void ApplyValue(float value) => _text.alpha = value;

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
                _text,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}