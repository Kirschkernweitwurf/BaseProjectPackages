using System;
using UnityEngine;

namespace Systems.Tweening.Core
{
    /// <summary>
    /// Non-generic base class that exposes behavior-level control for grouping/sequencing.
    /// </summary>
    public abstract class TweenBehaviourBase : MonoBehaviour
    {
        /// <summary>
        /// The currently active Tween instance (may be null).
        /// </summary>
        public abstract TweenBase ActiveTween { get; }

        /// <summary>
        /// Play the tween. If isReversed is true, the tween should go from 'to' to 'from'.
        /// </summary>
        /// <param name="isReversed">If true, tween should play in reverse.</param>
        public abstract void Play(bool isReversed);

        /// <summary>
        /// Stop playback immediately.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Fired when the behavior has fully finished (after loops / ping-pong / restarts are done).
        /// </summary>
        public abstract event Action OnFinished;
    }
}