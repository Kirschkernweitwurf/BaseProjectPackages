using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the fill amount of a UI Image from the current fill amount (captured at
    /// <c>Awake</c>) to a target fill amount.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageFillAmountToTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target fill amount to tween to.")]
        private float targetFillAmount = 1f;

        private Image _image;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => _image;

        protected override float StartValue => DefaultValue;

        protected override float LocalTargetValue => targetFillAmount;

#region Unity Callbacks
        protected override void Awake()
        {
            _image = GetComponent<Image>();

            base.Awake();
        }
#endregion

        protected override float GetCurrentValue() => _image.fillAmount;

        protected override void ApplyValue(float value) => _image.fillAmount = value;
    }
}