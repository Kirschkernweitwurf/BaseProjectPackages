using System;
using System.Collections.Generic;
using Base.AttributePackage;
using Base.CorePackage.MenuManaging;
using Base.CorePackage.Services.Shutdown;
using Base.CorePackage.Tweening.Core;
using Base.CorePackage.Tweening.Core.Data;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Base.CorePackage.Tweening.Components.System
{
    /// <summary>
    /// Groups multiple tweens together to play them as one sequence or in parallel.
    /// Provides instant-hide initialization and forward/reverse playback.
    /// </summary>
    public sealed class TweenGroup : MonoBehaviour, IShutdownHandler, IMenuResettable
    {
        /// <summary>
        /// Fired when the full sequence has finished (all tweens complete or stopped with
        /// <c>complete</c> set to <c>true</c>).
        /// </summary>
        public event Action OnFinished;

        /// <summary>
        /// Fired whenever the group stops, regardless of whether it completed or was killed.
        /// Always fires after <see cref="OnFinished"/> when both apply.
        /// </summary>
        public event Action OnKilled;

        [FormerlySerializedAs("tweenBehaviours")]
        [ComponentPicker]
        [Tooltip("The list of behaviours that should be played when showing the object.")]
        [SerializeField] private List<TweenBehaviourBase> showTweenBehaviours = new();

        [ComponentPicker]
        [Tooltip("The list of behaviours that should be played when hiding the object.")]
        [SerializeField] private List<TweenBehaviourBase> hideTweenBehaviours = new();

        [Header("Settings")]

        [Tooltip("If true, this group will auto-play once the object starts.")]
        [SerializeField] private bool playOnStart;

        [Tooltip("How the tweens in this group should be played.")]
        [SerializeField] private ESequenceMode sequenceMode;

        public bool HasShutDown { get; private set; }

        private TweenSequence _sequence;

#region Unity Callbacks
        private void Start()
        {
            ShutdownManager.Register(this);

            if (playOnStart)
                Show();
        }

        private void OnDestroy()
        {
            if (!HasShutDown)
                Shutdown();
        }
#endregion

        public void ResetState()
        {
            Stop();

            ResetBehaviours(showTweenBehaviours);
            ResetBehaviours(hideTweenBehaviours);
        }

        public void Shutdown()
        {
            ShutdownManager.Deregister(this);
            Stop();
            HasShutDown = true;
            OnFinished = null;
            OnKilled = null;
        }

        /// <summary>
        /// Sets the active state of the GameObject.
        /// </summary>
        public void SetVisibility(bool isVisible) => gameObject.SetActive(isVisible);

        /// <summary>
        /// Plays the show behaviors forward.
        /// </summary>
        public void Show() => PlayInternal(showTweenBehaviours, false);

        /// <summary>
        /// Plays the hide behaviors forward. If no hide behaviors are assigned, falls back to
        /// playing the show behaviors reversed so setups without a dedicated hide list still work.
        /// </summary>
        public void Hide() => PlayInternal(hideTweenBehaviours is
        {
            Count: > 0
        }
            ? hideTweenBehaviours
            : showTweenBehaviours, true);

        /// <summary>
        /// Stops all tweens in this group.
        /// </summary>
        /// <param name="complete">
        /// If <c>true</c>, each tween snaps to its end value and the group
        /// fires <see cref="OnFinished"/> before <see cref="OnKilled"/>. Useful to resolve gameplay
        /// logic that depends on the group finishing.
        /// </param>

        // ReSharper disable once MemberCanBePrivate.Global
        public void Stop(bool complete = false)
        {
            bool wasRunning = _sequence is
                {
                    IsRunning: true
                }
                || HasAnyActiveTween();

            if (_sequence is
                {
                    IsRunning: true
                })
            {
                _sequence.OnComplete -= HandleSequenceComplete;
                _sequence.Stop(complete);
                _sequence = null;
            }

            StopBehaviours(showTweenBehaviours, complete);
            StopBehaviours(hideTweenBehaviours, complete);

            if (!wasRunning)
                return;

            if (complete)
                OnFinished?.Invoke();

            OnKilled?.Invoke();
        }

        private static void StopBehaviours(List<TweenBehaviourBase> behaviours, bool complete)
        {
            foreach (TweenBehaviourBase behaviour in behaviours)
                behaviour?.Stop(complete);
        }

        private static bool HasActiveTween(List<TweenBehaviourBase> behaviours)
        {
            foreach (TweenBehaviourBase behaviour in behaviours)
            {
                if (behaviour != null
                    && behaviour.ActiveTween is
                    {
                        IsRunning: true
                    })
                    return true;
            }

            return false;
        }

        private static void ResetBehaviours(List<TweenBehaviourBase> behaviours)
        {
            foreach (TweenBehaviourBase behaviour in behaviours)
                behaviour?.ResetToDefault();
        }

        /// <summary>
        /// Plays the given behaviors in this group, either in sequence or in parallel based on the selected mode.
        /// </summary>
        /// <param name="behaviours">The behaviors to play.</param>
        /// <param name="isReversed">If <c>true</c>, plays the tweens reversed (from 'to' to 'from').</param>
        private void PlayInternal(List<TweenBehaviourBase> behaviours, bool isReversed)
        {
            if (behaviours == null || behaviours.Count == 0)
            {
                OnFinished?.Invoke();
                OnKilled?.Invoke();
                return;
            }

            Stop();

            _sequence = new TweenSequence(sequenceMode);

            if (!gameObject.activeInHierarchy)
                SetVisibility(true);

            foreach (TweenBehaviourBase behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    CustomLogger.LogWarning(
                        "Skipping null TweenBehaviour in TweenGroup on GameObject" + $" '{gameObject.name}'.", this);

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
            completedTween.OnComplete -= HandleSequenceComplete;
            _sequence = null;

            OnFinished?.Invoke();
            OnKilled?.Invoke();
        }

        private bool HasAnyActiveTween() => HasActiveTween(showTweenBehaviours) || HasActiveTween(hideTweenBehaviours);
    }
}