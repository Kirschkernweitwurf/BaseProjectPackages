using System;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Settings for looping tweens.
    /// </summary>
    /// <remarks>
    /// <see cref="LoopCount"/> is the number of additional loops <em>after</em> the initial play.
    /// For <see cref="ELoopType.PingPong"/>, each direction counts as one loop, so a value of 1
    /// gives one forward play and one reverse play.
    /// Use <c>-1</c> for infinite loops.
    /// </remarks>
    [Serializable]
    public class LoopSettings
    {
        [field: Tooltip("Number of additional loops after the initial play. Set to -1 for infinite loops.")]
        [field: SerializeField] public int LoopCount { get; private set; }

        [field: Tooltip("Type of looping behavior.")]
        [field: SerializeField] public ELoopType LoopType { get; private set; }

        /// <summary>Creates loop settings with the default values.</summary>
        public LoopSettings() { }

        /// <summary>Creates loop settings with the specified values.</summary>
        public LoopSettings(int loopCount, ELoopType loopType)
        {
            LoopCount = loopCount;
            LoopType = loopType;
        }

        /// <summary>
        /// Sets the number of additional loops after the initial play.
        /// </summary>
        public void SetLoopCount(int loopCount) => LoopCount = loopCount;

        /// <summary>
        /// Sets the looping behavior.
        /// </summary>
        public void SetLoopType(ELoopType loopType) => LoopType = loopType;

        /// <summary>
        /// Returns an independent copy of these settings. Used to keep runtime changes from
        /// leaking into shared assets.
        /// </summary>
        public LoopSettings Copy() => new(LoopCount, LoopType);
    }
}