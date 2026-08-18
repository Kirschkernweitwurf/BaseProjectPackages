using Base.AttributePackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using Base.TweeningPackage.Core.Data.Profiles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a TextMeshPro text between two fixed values (startColor → targetColor).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpColorTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting color to tween from.")]
        private Color startColor = Color.white;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        [GetComponent] [SerializeField] private TMP_Text text;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => text;

        protected override Color LocalStartValue => startColor;

        protected override Color LocalTargetValue => targetColor;

        protected override Color GetCurrentValue() => text.color;

        protected override void ApplyValue(Color value) => text.color = value;
    }
}