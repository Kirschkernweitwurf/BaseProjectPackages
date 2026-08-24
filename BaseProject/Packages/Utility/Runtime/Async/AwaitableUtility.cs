using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Base.UtilityPackage.Async
{
    /// <summary>
    /// Helpers for composing <see cref="Awaitable"/> operations: results that are already there,
    /// waiting for a whole batch and giving an operation a deadline.
    /// </summary>
    /// <remarks>
    /// An <see cref="Awaitable"/> is pooled and may only be awaited once, so every helper here
    /// awaits each one it is handed exactly once and never gives it back. All of them expect to be
    /// used from the main thread.
    /// </remarks>
    public static class AwaitableUtility
    {
        private const string TimeoutFormat = "The operation did not finish within {0} seconds.";

        /// <summary>
        /// An <see cref="Awaitable"/> that has already finished, so awaiting it resumes synchronously.
        /// </summary>
        /// <returns>A completed awaitable.</returns>
        public static Awaitable Completed()
        {
            AwaitableCompletionSource source = new();
            source.SetResult();

            return source.Awaitable;
        }

        /// <summary>
        /// An <see cref="Awaitable{T}"/> that has already finished with the given result.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="result">The result to hand back.</param>
        /// <returns>A completed awaitable carrying <paramref name="result"/>.</returns>
        public static Awaitable<T> FromResult<T>(T result)
        {
            AwaitableCompletionSource<T> source = new();
            source.SetResult(result);

            return source.Awaitable;
        }

        /// <summary>Waits until every given operation has finished.</summary>
        /// <param name="awaitables">The running operations. Null entries are skipped.</param>
        /// <returns>An awaitable that finishes with the slowest of them.</returns>
        public static Awaitable WhenAll(params Awaitable[] awaitables) => WhenAll((IEnumerable<Awaitable>)awaitables);

        /// <summary>
        /// Waits until every given operation has finished, whether the others fail or not.
        /// </summary>
        /// <param name="awaitables">The running operations. Null entries are skipped.</param>
        /// <returns>An awaitable that finishes with the slowest of them.</returns>
        /// <exception cref="ArgumentNullException">When the collection is null.</exception>
        /// <exception cref="AggregateException">When more than one operation failed.</exception>
        /// <exception cref="OperationCanceledException">
        /// When at least one operation was canceled and none of them failed.
        /// </exception>
        public static async Awaitable WhenAll(IEnumerable<Awaitable> awaitables)
        {
            if (awaitables == null)
                throw new ArgumentNullException(nameof(awaitables));

            AwaitableFailures failures = new();

            // The operations are already running, so awaiting them one after the other still ends
            // with the slowest one, and each is awaited exactly once as the pool requires.
            foreach (Awaitable awaitable in awaitables)
            {
                if (awaitable == null)
                    continue;

                try
                {
                    await awaitable;
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }

            failures.Rethrow();
        }

        /// <summary>
        /// Waits until every given operation has finished and collects their results in order.
        /// </summary>
        /// <typeparam name="T">The type of the results.</typeparam>
        /// <param name="awaitables">The running operations. A null entry yields the default result.</param>
        /// <returns>The results, in the order the operations were given.</returns>
        /// <exception cref="ArgumentNullException">When the array is null.</exception>
        /// <exception cref="AggregateException">When more than one operation failed.</exception>
        /// <exception cref="OperationCanceledException">
        /// When at least one operation was canceled and none of them failed.
        /// </exception>
        public static async Awaitable<T[]> WhenAll<T>(params Awaitable<T>[] awaitables)
        {
            if (awaitables == null)
                throw new ArgumentNullException(nameof(awaitables));

            T[] results = new T[awaitables.Length];
            AwaitableFailures failures = new();

            for (int i = 0; i < awaitables.Length; i++)
            {
                if (awaitables[i] == null)
                    continue;

                try
                {
                    results[i] = await awaitables[i];
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
            }

            failures.Rethrow();

            return results;
        }

        /// <summary>
        /// Runs an operation under a deadline. The token handed to <paramref name="operation"/> is
        /// canceled once the deadline passes, so the work actually stops instead of running on
        /// unwatched. The operation therefore has to observe that token for the deadline to bite.
        /// </summary>
        /// <param name="operation">Starts the work with the token that carries the deadline.</param>
        /// <param name="seconds">How long the operation may take. Counted on the player loop.</param>
        /// <param name="cancellationToken">Cancels the operation and the deadline together.</param>
        /// <remarks>
        /// The token is canceled again once this call returns, so nothing the operation started may
        /// outlive it.
        /// </remarks>
        /// <exception cref="ArgumentNullException">When the operation is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the deadline is negative.</exception>
        /// <exception cref="TimeoutException">When the deadline passed before the work was done.</exception>
        /// <exception cref="OperationCanceledException">
        /// When <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        public static async Awaitable WithTimeout(Func<CancellationToken, Awaitable> operation, float seconds,
            CancellationToken cancellationToken = default)
        {
            Validate(operation, seconds);

            using AwaitableTimeout timeout = new(seconds, cancellationToken);

            try
            {
                await operation(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.HasExpired)
            {
                throw Expired(seconds);
            }

            // An operation that ignores its token can still come back after the deadline. The caller
            // asked for a deadline, so that outcome is reported as one too rather than as a success.
            if (timeout.HasExpired)
                throw Expired(seconds);
        }

        /// <summary>
        /// Runs an operation under a deadline and returns its result. The token handed to
        /// <paramref name="operation"/> is canceled once the deadline passes, so the work actually
        /// stops instead of running on unwatched.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="operation">Starts the work with the token that carries the deadline.</param>
        /// <param name="seconds">How long the operation may take. Counted on the player loop.</param>
        /// <param name="cancellationToken">Cancels the operation and the deadline together.</param>
        /// <returns>The result of the operation.</returns>
        /// <remarks>
        /// The token is canceled again once this call returns, so nothing the operation started may
        /// outlive it.
        /// </remarks>
        /// <exception cref="ArgumentNullException">When the operation is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the deadline is negative.</exception>
        /// <exception cref="TimeoutException">When the deadline passed before the work was done.</exception>
        /// <exception cref="OperationCanceledException">
        /// When <paramref name="cancellationToken"/> was canceled.
        /// </exception>
        public static async Awaitable<T> WithTimeout<T>(Func<CancellationToken, Awaitable<T>> operation,
            float seconds, CancellationToken cancellationToken = default)
        {
            Validate(operation, seconds);

            using AwaitableTimeout timeout = new(seconds, cancellationToken);

            T result;

            try
            {
                result = await operation(timeout.Token);
            }
            catch (OperationCanceledException) when (timeout.HasExpired)
            {
                throw Expired(seconds);
            }

            if (timeout.HasExpired)
                throw Expired(seconds);

            return result;
        }

        // Typed as a delegate so both overloads share it. The parameter name is what the thrown
        // exception reports, so it matches the public one it stands in for.
        private static void Validate(Delegate operation, float seconds)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (seconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(seconds));
        }

        private static TimeoutException Expired(float seconds) => new(string.Format(TimeoutFormat, seconds));
    }
}