using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of any UI Graphic from the current color (captured at <c>Awake</c>)
    /// to a target color.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class GraphicColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Graphic _graphic;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => _graphic;

        protected override Color StartValue => DefaultValue;

        protected override Color LocalTargetValue => targetColor;

#region Unity Callbacks
        protected override void Awake()
        {
            _graphic = GetComponent<Graphic>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _graphic.color;

        protected override void ApplyValue(Color value) => _graphic.color = value;
    }
}