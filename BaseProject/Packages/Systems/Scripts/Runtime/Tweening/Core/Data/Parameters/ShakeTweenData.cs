using System;
using UnityEngine;

namespace Systems.Tweening.Core.Data.Parameters
{
    /// <summary>
    /// Serializable data describing a shake tween for transforms.
    /// </summary>
    [Serializable]
    public struct ShakeTweenData
    {
        [field: Tooltip("Maximum offset distance per tick.")]
        [field: SerializeField] public float Strength { get; private set; }

        [field: Tooltip("Basic tween parameters.")]
        [field: SerializeField] public TweenData TweenData { get; private set; }

        public ShakeTweenData(float strength, TweenData tweenData)
        {
            Strength = strength;
            TweenData = tweenData;
        }
    }
}