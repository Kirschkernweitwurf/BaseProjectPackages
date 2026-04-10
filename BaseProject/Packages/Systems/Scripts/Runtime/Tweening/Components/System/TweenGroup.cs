using System;
using System.Collections.Generic;
using Systems.Shutdown;
using Systems.Tweening.Core;
using Systems.Tweening.Core.Data;
using UnityEngine;
using Utility.Logging;

namespace Systems.Tweening.Components.System
{
    /// <summary>
    /// Groups multiple tweens together to play them as one sequence or in parallel.
    /// Provides instant-hide initialization and forward/reverse playback.
    /// </summary>
    public sealed class TweenGroup : MonoBehaviour, IShutdownHandler
    {
        [Tooltip("The list of TweenBehaviours included in this group.")]
        [SerializeField] private List<TweenBehaviourBase> tweenBehaviours = new();

        [Header("Settings")]
        [Tooltip("If true, this group will auto-play once the object starts.")]
        [SerializeField] private bool playOnStart;

        [Tooltip("How the tweens in this group should be played.")]
        [SerializeField] private ESequenceMode sequenceMode;

        public bool HasShutDown { get; private set; }

        private TweenSequence _sequence;

        /// <summary>
        /// Fired when the full sequence has finished (all tweens complete).
        /// </summary>
        public event Action OnFinished;

        private void Start()
        {
            ShutdownManager.Register(this);

            if (playOnStart)
                Play();
        }

        private void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);
            Stop();
            HasShutDown = true;
            OnFinished = null;
        }

        /// <summary>
        /// Sets the active state of the GameObject.
        /// </summary>
        public void SetVisibility(bool isVisible) => gameObject.SetActive(isVisible);

        /// <summary>
        /// Play the group forward.
        /// </summary>
        public void Play() => PlayInternal(isReversed: false);

        /// <summary>
        /// Play the group in reverse.
        /// </summary>
        public void Reverse() => PlayInternal(isReversed: true);

        /// <summary>
        /// Stops all tweens in this group.
        /// </summary>
        public void Stop()
        {
            if (_sequence is { IsRunning: true })
                _sequence.Stop();

            foreach (TweenBehaviourBase behaviour in tweenBehaviours)
                behaviour?.Stop();
        }

        /// <summary>
        /// Plays all tweens in this group, either in sequence or in parallel based on the selected mode.
        /// </summary>
        /// <param name="isReversed">If <c>true</c>, plays the tweens reversed (from 'to' to 'from').</param>
        private void PlayInternal(bool isReversed = false)
        {
            if (tweenBehaviours == null || tweenBehaviours.Count == 0)
            {
                HandleSequenceComplete(_sequence);
                return;
            }

            Stop();

            _sequence = new TweenSequence(sequenceMode);

            if (!gameObject.activeInHierarchy)
                SetVisibility(true);

            foreach (TweenBehaviourBase behaviour in tweenBehaviours)
            {
                if (behaviour == null)
                {
                    CustomLogger.LogWarning("Skipping null TweenBehaviour in TweenGroup on GameObject" +
                                            $" '{gameObject.name}'.", this);
                    continue;
                }

                behaviour.Stop();
                behaviour.Play(isReversed);

                if (behaviour.ActiveTween != null)
                    _sequence.Add(behaviour.ActiveTween);
            }

            _sequence.OnComplete += HandleSequenceComplete;
            _sequence.Start();
        }

        private void HandleSequenceComplete(TweenBase completedTween)
        {
            if (completedTween != null)
                completedTween.OnComplete -= HandleSequenceComplete;

            OnFinished?.Invoke();
        }
    }
}