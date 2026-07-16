using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.CorePackage.Tweening.Core.Data.Profiles;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.CorePackage.Tweening.Components.UITweens
{
    /// <summary>
    /// Tweens the alpha of a TextMeshPro text from the current alpha (captured at <c>Awake</c>)
    /// to a target alpha.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TmpAlphaToTween : TweenBehaviour<float>
    {
        [SerializeField] [Tooltip("The profile driving this tween, used while the profile toggle is on.")]
        private FloatTweenProfileSo profile;

        [SerializeField] [TweenValue] [Tooltip("The target alpha value to tween to.")]
        private float targetAlpha = 1f;

        private TMP_Text _text;

        protected override TweenValueProfileSo<float> ProfileAsset => profile;

        protected override Object TweenTarget => _text;

        protected override float StartValue => DefaultValue;

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