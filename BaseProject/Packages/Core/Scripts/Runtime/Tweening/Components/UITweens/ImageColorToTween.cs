using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the color of a UI Image from the current color (captured at <c>Awake</c>)
    /// to a target color.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageColorToTween : TweenBehaviour<Color>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private ColorTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target color to tween to.")]
        private Color targetColor = Color.white;

        private Image _image;

        protected override TweenValueProfileSo<Color> ProfileAsset => profile;

        protected override Object TweenTarget => _image;

        protected override Color StartValue => DefaultValue;

        protected override Color LocalTargetValue => targetColor;

#region Unity Callbacks
        protected override void Awake()
        {
            _image = GetComponent<Image>();

            base.Awake();
        }
#endregion

        protected override Color GetCurrentValue() => _image.color;

        protected override void ApplyValue(Color value) => _image.color = value;
    }
}