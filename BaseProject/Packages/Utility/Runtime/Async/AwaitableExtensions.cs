using System;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.UtilityPackage.Async
{
    /// <summary>
    /// Extensions for the two cases a plain <c>await</c> does not cover: nobody is waiting for the
    /// operation, and cancellation is an expected outcome rather than an error.
    /// </summary>
    public static class AwaitableExtensions
    {
        private const string FailureFormat = "A detached {0} failed with nobody awaiting it. {1}";

        /// <summary>
        /// Awaits an operation nobody is waiting for. Without this a failure is swallowed and the
        /// awaitable never returns to Unity's pool, so this is how a fire and forget call is made.
        /// </summary>
        /// <param name="awaitable">The operation to observe.</param>
        public static async void Forget(this Awaitable awaitable)
        {
            if (awaitable == null)
                return;

            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
                // Cancellation is how a detached operation is meant to end early.
            }
            catch (Exception exception)
            {
                CustomLogger.LogError(string.Format(FailureFormat, nameof(Awaitable), exception), null);
            }
        }

        /// <summary>
        /// Awaits an operation nobody is waiting for and drops its result. Without this a failure is
        /// swallowed and the awaitable never returns to Unity's pool.
        /// </summary>
        /// <typeparam name="T">The type of the dropped result.</typeparam>
        /// <param name="awaitable">The operation to observe.</param>
        public static void Forget<T>(this Awaitable<T> awaitable)
        {
            if (awaitable == null)
                return;

            Drop(awaitable).Forget();
        }

        /// <summary>
        /// Awaits an operation whose cancellation is expected, reporting it instead of throwing.
        /// Everything else still throws, so a real failure is not swallowed along with it.
        /// </summary>
        /// <param name="awaitable">The operation to await.</param>
        /// <returns>True if the operation ran to the end; false if it was canceled or null.</returns>
        public static async Awaitable<bool> HasCompleted(this Awaitable awaitable)
        {
            if (awaitable == null)
                return false;

            try
            {
                await awaitable;
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        // Sheds the result type, so the untyped overload above can do the observing for both.
        private static async Awaitable Drop<T>(Awaitable<T> awaitable) => await awaitable;
    }
}