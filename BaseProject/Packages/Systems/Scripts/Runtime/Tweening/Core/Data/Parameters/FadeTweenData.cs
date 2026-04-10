using System;
using UnityEngine;

namespace Systems.Tweening.Core.Data.Parameters
{
    /// <summary>
    /// Serializable data describing a fade tween for a CanvasGroup.
    /// </summary>
    [Serializable]
    public struct FadeTweenData
    {
        [field: Tooltip("Target alpha value (0–1).")]
        [field: SerializeField] public float TargetAlpha { get; private set; }

        [field: Tooltip("Basic tween parameters.")]
        [field: SerializeField] public TweenData TweenData { get; private set; }

        public FadeTweenData(float targetAlpha, TweenData tweenData)
        {
            TargetAlpha = targetAlpha;
            TweenData = tweenData;
        }
    }
}