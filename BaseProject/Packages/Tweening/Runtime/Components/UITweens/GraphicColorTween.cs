using Base.AttributesPackage;
using Base.TweeningPackage.Core;
using Base.TweeningPackage.Core.Data;
using Base.TweeningPackage.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.TweeningPackage.Components.UITweens
{
    /// <summary>
    /// Tweens the color of any UI Graphic between two fixed values (startColor → targetColor).
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class GraphicColorTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting color to tween from.")]
        private Color startColor = Color.white;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        [GetComponent] [SerializeField] private Graphic graphic;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => graphic;

        protected override Color LocalStartValue => startColor;

        protected override Color LocalTargetValue => targetColor;

        protected override Color GetCurrentValue() => graphic.color;

        protected override void ApplyValue(Color value) => graphic.color = value;
    }
}