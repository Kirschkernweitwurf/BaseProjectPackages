using System;
using UnityEngine;

namespace Base.TweeningPackage.Core.Data.Parameters
{
    /// <summary>
    /// Serializable data describing a shake tween for transforms.
    /// </summary>
    [Serializable]
    public struct ShakeTweenData
    {
        [field: Tooltip("Maximum offset distance per tick.")]
        [field: Min(0f)] [field: SerializeField] public float Strength { get; private set; }

        [field: Tooltip("Basic tween parameters.")]
        [field: SerializeField] public TweenData TweenData { get; private set; }

        /// <summary>Creates the parameters for a shake.</summary>
        /// <param name="strength">How far the shake displaces the target at its peak.</param>
        /// <param name="tweenData">The duration, easing and delay the shake runs with.</param>
        public ShakeTweenData(float strength, TweenData tweenData)
        {
            Strength = strength;
            TweenData = tweenData;
        }
    }
}