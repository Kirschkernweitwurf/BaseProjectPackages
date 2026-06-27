using Base.SystemsCorePackage.Tweening.Core;
using Base.SystemsCorePackage.Tweening.Core.Data;
using TMPro;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a TextMeshPro text between two fixed values (startColor → targetColor).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpColorTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The starting color to tween from.")]
        private Color startColor = Color.white;

        [SerializeField] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private TMP_Text _text;

#region Unity Callbacks
        protected override void Awake()
        {
            _text = GetComponent<TMP_Text>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _text.color;

        protected override void ApplyValue(Color value) => _text.color = value;

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
                _text,
                TweenSettings.Delay,
                fromGetter: () => from);
        }
    }
}