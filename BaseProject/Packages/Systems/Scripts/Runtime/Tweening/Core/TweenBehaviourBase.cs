using System;
using UnityEngine;

namespace Base.SystemsCorePackage.Tweening.Core
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
        /// Stop playback. By default the behaviour is killed without firing <c>OnFinished</c>.
        /// When <paramref name="complete"/> is <c>true</c>, the active tween snaps to its end value
        /// and <c>OnFinished</c> is fired before <c>OnKilled</c> — useful to resolve gameplay
        /// logic that depends on a tween finishing.
        /// </summary>
        /// <param name="complete">If <c>true</c>, complete the active tween instead of just killing it.</param>
        public abstract void Stop(bool complete = false);

        /// <summary>
        /// Fired when the behaviour has fully finished (after loops / ping-pong / restarts are done,
        /// or after <c>Stop(complete: true)</c>).
        /// </summary>
        public abstract event Action OnFinished;

        /// <summary>
        /// Fired whenever the behaviour stops, regardless of whether it completed or was killed.
        /// Always fires after <c>OnFinished</c> when both apply.
        /// </summary>
        public abstract event Action OnKilled;
    }
}
