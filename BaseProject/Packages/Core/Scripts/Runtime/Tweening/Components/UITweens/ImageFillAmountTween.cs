using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the fill amount of a UI Image between two fixed values
    /// (startFillAmount → targetFillAmount).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ImageFillAmountTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting fill amount to tween from.")]
        private float startFillAmount;

        [SerializeField] [TweenValue] [Tooltip("The target fill amount to tween to.")]
        private float targetFillAmount = 1f;

        private Image _image;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => _image;

        protected override float LocalStartValue => startFillAmount;

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