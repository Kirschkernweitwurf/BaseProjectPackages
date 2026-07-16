using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a TextMeshPro text from the current color (captured at <c>Awake</c>)
    /// to a target color.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private TMP_Text _text;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => _text;

        protected override Color StartValue => DefaultValue;

        protected override Color LocalTargetValue => targetColor;

#region Unity Callbacks
        protected override void Awake()
        {
            _text = GetComponent<TMP_Text>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _text.color;

        protected override void ApplyValue(Color value) => _text.color = value;
    }
}