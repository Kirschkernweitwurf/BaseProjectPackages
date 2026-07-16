using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a TextMeshPro text between two fixed values (startAlpha → targetAlpha).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpAlphaTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The starting alpha value.")]
        private float startAlpha;

        [SerializeField] [TweenValue] [Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        private TMP_Text _text;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => _text;

        protected override float LocalStartValue => startAlpha;

        protected override float LocalTargetValue => targetAlpha;

#region Unity Callbacks
        protected override void Awake()
        {
            _text = GetComponent<TMP_Text>();

            base.Awake();
        }
#endregion

        protected override float GetCurrentValue() => _text.alpha;

        protected override void ApplyValue(float value) => _text.alpha = value;
    }
}