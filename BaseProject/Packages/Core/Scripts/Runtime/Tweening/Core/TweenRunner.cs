using System;
using System.Collections.Generic;
using Base.CorePackage.Services;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach

namespace Base.CorePackage.Tweening.Core
{
    /// <summary>
    /// Central update service that manages and advances all active tweens.
    /// Broadcasts tween lifecycle events for other systems to react to.
    /// </summary>
    public sealed class TweenRunner : GameServiceBehaviour
    {
        /// <summary>
        /// Invoked when a tween is registered and begins updating.
        /// </summary>
        public static event Action<ITween> OnTweenRegistered;

        /// <summary>
        /// Invoked once per frame while a tween is active.
        /// </summary>
        public static event Action<ITween> OnTweenUpdated;

        /// <summary>
        /// Invoked when a tween completes and is removed from the active list.
        /// </summary>
        public static event Action<ITween> OnTweenDeregistered;

        [Header("Delta Time Spike Handling")]

        [Tooltip("Max unscaled dt step used for tween progression. Prevents hitch/breakpoint fast-forward.")]
        [SerializeField] private float maxUnscaledDeltaTime = 0.05f;

        [Tooltip("If true, the first Update after app regains focus/unpauses will not advance tweens.")]
        [SerializeField] private bool skipFirstFrameAfterFocusOrPause = true;

        private readonly List<ITween> _tweens = new();
        private readonly HashSet<ITween> _activeSet = new();
        private readonly List<ITween> _pendingRemovals = new();
        private readonly HashSet<ITween> _pendingRemovalSet = new();
        private readonly List<ITween> _pendingAdditions = new();
        private readonly HashSet<ITween> _pendingAdditionSet = new();

        private bool _skipNextUpdate;
        private bool _isUpdating;

#region Unity Callbacks
        private void Update()
        {
            if (_skipNextUpdate)
            {
                _skipNextUpdate = false;
                ProcessTweens(0f);
                return;
            }

            float dt = Time.unscaledDeltaTime;

            if (maxUnscaledDeltaTime > 0f && dt > maxUnscaledDeltaTime)
                dt = maxUnscaledDeltaTime;

            ProcessTweens(dt);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
                return;

            if (skipFirstFrameAfterFocusOrPause)
                _skipNextUpdate = true;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                return;

            if (skipFirstFrameAfterFocusOrPause)
                _skipNextUpdate = true;
        }
#endregion

        /// <summary>
        /// Registers a tween to be updated each frame.
        /// </summary>
        public void RegisterTween(ITween tween)
        {
            if (tween == null)
                return;

            if (_isUpdating)
            {
                if (!_activeSet.Contains(tween) && _pendingAdditionSet.Add(tween))
                    _pendingAdditions.Add(tween);

                return;
            }

            if (!_activeSet.Add(tween))
                return;

            _tweens.Add(tween);
            OnTweenRegistered?.Invoke(tween);
        }

        /// <summary>
        /// Unregisters a tween so it no longer receives updates.
        /// </summary>
        public void UnregisterTween(ITween tween)
        {
            if (tween == null)
                return;

            if (!_isUpdating)
            {
                if (!_activeSet.Remove(tween))
                    return;

                _tweens.Remove(tween);
                OnTweenDeregistered?.Invoke(tween);
                return;
            }

            if (_activeSet.Contains(tween) && _pendingRemovalSet.Add(tween))
                _pendingRemovals.Add(tween);
        }

        private void ProcessTweens(float deltaTime)
        {
            _isUpdating = true;

            for (int i = 0; i < _tweens.Count; i++)
            {
                ITween tween = _tweens[i];

                tween.Tick(deltaTime);
                OnTweenUpdated?.Invoke(tween);

                if (tween.IsCompleted && _pendingRemovalSet.Add(tween))
                    _pendingRemovals.Add(tween);
            }

            _isUpdating = false;

            // Apply removals.
            if (_pendingRemovals.Count > 0)
            {
                for (int i = 0; i < _pendingRemovals.Count; i++)
                {
                    ITween tween = _pendingRemovals[i];
                    if (!_activeSet.Remove(tween))
                        continue;

                    _tweens.Remove(tween);
                    OnTweenDeregistered?.Invoke(tween);
                }

                _pendingRemovals.Clear();
                _pendingRemovalSet.Clear();
            }

            // Apply additions.
            if (_pendingAdditions.Count == 0)
                return;

            for (int i = 0; i < _pendingAdditions.Count; i++)
            {
                ITween tween = _pendingAdditions[i];
                if (tween == null || !_activeSet.Add(tween))
                    continue;

                _tweens.Add(tween);
                OnTweenRegistered?.Invoke(tween);
            }

            _pendingAdditions.Clear();
            _pendingAdditionSet.Clear();
        }
    }
}