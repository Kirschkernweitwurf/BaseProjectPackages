using System;
using Base.AttributePackage;
using UnityEngine;

namespace Base.TweeningPackage.Core.Data.Parameters
{
    /// <summary>
    /// Serializable data describing a fade tween for a CanvasGroup.
    /// </summary>
    [Serializable]
    public struct FadeTweenData
    {
        [field: Tooltip("Target alpha value (0 to 1).")]
        [field: MinMax(0f, 1f)] [field: SerializeField] public float TargetAlpha { get; private set; }

        [field: Tooltip("Basic tween parameters.")]
        [field: SerializeField] public TweenData TweenData { get; private set; }

        /// <summary>Creates the parameters for a fade.</summary>
        /// <param name="targetAlpha">The alpha the target ends on.</param>
        /// <param name="tweenData">The duration, easing and delay the fade runs with.</param>
        public FadeTweenData(float targetAlpha, TweenData tweenData)
        {
            TargetAlpha = targetAlpha;
            TweenData = tweenData;
        }
    }
}