using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base.UtilityPackage.Logging;

namespace Base.UtilityPackage
{
    /// <summary>
    /// A centralized <see cref="MonoBehaviour"/> for running and managing coroutines.
    /// Allows non-MonoBehaviour classes to start coroutines and provides convenience utilities for delayed actions.
    /// </summary>
    public class CoroutineRunner : CustomSingleton<CoroutineRunner>
    {
        private readonly List<Coroutine> _coroutines = new();

        /// <summary>
        /// Ensures all coroutines are stopped when this component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            StopAllCoroutines();
            _coroutines.Clear();
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting for the given number of frames.
        /// </summary>
        /// <param name="actionToRun">The action to run after the specified number of frames.</param>
        /// <param name="frameCount">The number of frames to wait before running the action.</param>
        private static IEnumerator RunAfterFramesCoroutine(Action actionToRun, int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
                yield return null;

            actionToRun?.Invoke();
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting for the provided
        /// <paramref name="yieldInstruction"/>.
        /// </summary>
        /// <param name="actionToRun">The action to run after the wait instruction completes.</param>
        /// <param name="yieldInstruction">The yield instruction to wait for before executing the action.</param>
        private static IEnumerator RunActionDelayed(Action actionToRun, YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;
            actionToRun?.Invoke();
        }

        /// <summary>
        /// Starts a coroutine and adds it to the list of tracked coroutines.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunCoroutine(IEnumerator coroutine)
        {
            Coroutine newCoroutine = StartCoroutine(coroutine);
            _coroutines.Add(newCoroutine);
            return newCoroutine;
        }

        /// <summary>
        /// Starts a coroutine and invokes a callback when it completes.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <param name="onComplete">The callback to invoke once the coroutine finishes.</param>
        /// <returns>An enumerator suitable for yielding in another coroutine.</returns>
        public Coroutine RunCoroutine(IEnumerator coroutine, Action onComplete)
        {
            Coroutine newCoroutine = StartCoroutine(RunCoroutineWithCallback(coroutine, onComplete));
            _coroutines.Add(newCoroutine);
            return newCoroutine;

            IEnumerator RunCoroutineWithCallback(IEnumerator coro, Action callback)
            {
                yield return StartCoroutineInternal(coro);
                callback?.Invoke();
            }
        }

        /// <summary>
        /// Starts a coroutine after waiting one frame.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunCoroutineNextFrame(IEnumerator coroutine)
        {
            Coroutine newCoroutine = StartCoroutine(DelayedStart());
            _coroutines.Add(newCoroutine);
            return newCoroutine;

            IEnumerator DelayedStart()
            {
                yield return null; // Wait one frame
                yield return StartCoroutineInternal(coroutine);
            }
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting one frame.
        /// </summary>
        /// <param name="actionToRun">The action to execute the next frame.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunNextFrame(Action actionToRun)
        {
            Coroutine newCoroutine = StartCoroutine(RunActionDelayed(actionToRun, new WaitForSeconds(0)));
            _coroutines.Add(newCoroutine);
            return newCoroutine;
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after a certain number of frames.
        /// </summary>
        /// <param name="actionToRun">The action to execute after waiting.</param>
        /// <param name="frameCount">The number of frames to wait. Must be non-negative.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        public Coroutine RunAfterFrames(Action actionToRun, int frameCount)
        {
            if (frameCount < 0)
            {
                CustomLogger.LogWarning("Frame count must be non-negative.", this);
                frameCount = 0;
            }

            if (frameCount == 0)
            {
                actionToRun?.Invoke();
                return null;
            }

            Coroutine newCoroutine = StartCoroutine(RunAfterFramesCoroutine(actionToRun, frameCount));
            _coroutines.Add(newCoroutine);
            return newCoroutine;
        }

        /// <summary>
        /// Runs the specified <paramref name="actionToRun"/> after waiting a given number of seconds.
        /// </summary>
        /// <param name="actionToRun">The action to execute after the delay.</param>
        /// <param name="seconds">The number of seconds to wait before execution. Must be non-negative.</param>
        /// <returns>The <see cref="Coroutine"/> instance started.</returns>
        /// <remarks>
        /// If <paramref name="seconds"/> is less than or equal to zero, the action will execute on the next frame.
        /// </remarks>
        public Coroutine RunAfterSeconds(Action actionToRun, float seconds)
        {
            if (seconds < 0f)
            {
                CustomLogger.LogWarning("Seconds must be non-negative.", this);
                seconds = 0f;
            }

            if (Mathf.Approximately(seconds, 0f))
            {
                actionToRun?.Invoke();
                return null;
            }

            Coroutine newCoroutine = StartCoroutine(RunActionDelayed(actionToRun, new WaitForSeconds(seconds)));
            _coroutines.Add(newCoroutine);
            return newCoroutine;
        }

        /// <summary>
        /// Stops a specific running coroutine and removes it from the tracked list.
        /// </summary>
        /// <param name="coroutine">The coroutine instance to stop.</param>
        public void StopRunningCoroutine(Coroutine coroutine)
        {
            if (!_coroutines.Contains(coroutine))
                return;

            StopCoroutine(coroutine);
            _coroutines.Remove(coroutine);
        }

        /// <summary>
        /// Stops all currently running coroutines and clears the tracked list.
        /// </summary>
        public void StopAllRunningCoroutines()
        {
            StopAllCoroutines();
            _coroutines.Clear();
        }

        /// <summary>
        /// Starts a coroutine internally and tracks it, returning its enumerator.
        /// </summary>
        /// <param name="coroutine">The coroutine enumerator to run.</param>
        /// <returns>An enumerator yielding until the coroutine completes.</returns>
        private IEnumerator StartCoroutineInternal(IEnumerator coroutine)
        {
            Coroutine subCoroutine = StartCoroutine(coroutine);
            _coroutines.Add(subCoroutine);
            yield return subCoroutine;
        }
    }
}