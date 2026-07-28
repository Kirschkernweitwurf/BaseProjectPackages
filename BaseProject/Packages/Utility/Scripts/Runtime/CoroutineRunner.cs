using System;
using System.Collections;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.UtilityPackage
{
    /// <summary>
    /// A centralized <see cref="MonoBehaviour"/> for running and managing coroutines.
    /// Allows non-MonoBehaviour classes to start coroutines and provides convenience utilities for delayed actions.
    /// </summary>
    public class CoroutineRunner : CustomSingleton<CoroutineRunner>
    {
        private const int FramesForNextFrame = 1;

        private readonly HashSet<Coroutine> _coroutines = new();

#region Unity Callbacks
        private void OnDestroy()
        {
            StopAllCoroutines();
            _coroutines.Clear();
        }
#endregion

        /// <summary>
        /// Starts a coroutine and adds it to the set of tracked coroutines.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunCoroutine(IEnumerator coroutine) => StartTracked(coroutine);

        /// <summary>
        /// Starts a coroutine and invokes a callback when it completes.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <param name="onComplete">The callback to invoke once the coroutine finishes.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunCoroutine(IEnumerator coroutine, Action onComplete)
        {
            return StartTracked(RunWithCallback());

            IEnumerator RunWithCallback()
            {
                while (coroutine.MoveNext())
                    yield return coroutine.Current;

                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Starts a coroutine after waiting one frame.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunCoroutineNextFrame(IEnumerator coroutine)
        {
            return StartTracked(DelayedStart());

            IEnumerator DelayedStart()
            {
                yield return null;

                while (coroutine.MoveNext())
                    yield return coroutine.Current;
            }
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting one frame.
        /// </summary>
        /// <param name="actionToRun">The action to execute the next frame.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunNextFrame(Action actionToRun)
            => StartTracked(RunAfterFramesCoroutine(actionToRun, FramesForNextFrame));

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after a certain number of frames.
        /// </summary>
        /// <param name="actionToRun">The action to execute after waiting.</param>
        /// <param name="frameCount">The number of frames to wait. Must be non-negative.</param>
        /// <returns>
        /// The <see cref="Coroutine"/> instance started, or <c>null</c> if the action ran immediately.
        /// </returns>
        public Coroutine RunAfterFrames(Action actionToRun, int frameCount)
        {
            if (frameCount < 0)
            {
                CustomLogger.LogWarning($"{nameof(frameCount)} must be non-negative.", this);
                frameCount = 0;
            }

            if (frameCount == 0)
            {
                actionToRun?.Invoke();
                return null;
            }

            return StartTracked(RunAfterFramesCoroutine(actionToRun, frameCount));
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting a given number of seconds.
        /// </summary>
        /// <param name="actionToRun">The action to execute after the delay.</param>
        /// <param name="seconds">The number of seconds to wait before execution. Must be non-negative.</param>
        /// <returns>
        /// The <see cref="Coroutine"/> instance started, or <c>null</c> if the action ran immediately.
        /// </returns>
        /// <remarks>
        /// If <paramref name="seconds"/> is less than or equal to zero, the action will execute immediately.
        /// </remarks>
        public Coroutine RunAfterSeconds(Action actionToRun, float seconds)
        {
            if (seconds < 0f)
            {
                CustomLogger.LogWarning($"{nameof(seconds)} must be non-negative.", this);
                seconds = 0f;
            }

            if (Mathf.Approximately(seconds, 0f))
            {
                actionToRun?.Invoke();
                return null;
            }

            return StartTracked(RunActionDelayed(actionToRun, new WaitForSeconds(seconds)));
        }

        /// <summary>
        /// Stops a specific running coroutine and removes it from the tracked set.
        /// </summary>
        /// <param name="coroutine">The coroutine instance to stop.</param>
        public void StopRunningCoroutine(Coroutine coroutine)
        {
            if (coroutine == null
                || !_coroutines.Remove(coroutine))
                return;

            StopCoroutine(coroutine);
        }

        /// <summary>
        /// Stops all currently running coroutines and clears the tracked set.
        /// </summary>
        public void StopAllRunningCoroutines()
        {
            StopAllCoroutines();
            _coroutines.Clear();
        }

#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void ResetStatics() => Instance = null;
#endif

        private static IEnumerator RunAfterFramesCoroutine(Action actionToRun, int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
                yield return null;

            actionToRun?.Invoke();
        }

        private static IEnumerator RunActionDelayed(Action actionToRun, YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;

            actionToRun?.Invoke();
        }

        /// <summary>
        /// Starts <paramref name="coroutine"/> wrapped in a tracker that removes it from the set on completion.
        /// Stopping the returned handle also stops the wrapped coroutine, since it is iterated inline.
        /// </summary>
        private Coroutine StartTracked(IEnumerator coroutine)
        {
            Coroutine handle = null;
            handle = StartCoroutine(Tracked());
            _coroutines.Add(handle);
            return handle;

            IEnumerator Tracked()
            {
                while (coroutine.MoveNext())
                    yield return coroutine.Current;

                _coroutines.Remove(handle);
            }
        }
    }
}
