using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using TMPro;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a TextMeshPro text from the current alpha (captured at <c>Awake</c>)
    /// to a target alpha.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpAlphaToTween : TweenBehaviour<float>
    {
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
                : DefaultValue;

            float to = isReversed
                ? DefaultValue
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