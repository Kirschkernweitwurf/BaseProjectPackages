using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace Base.UtilityPackage.Async
{
    /// <summary>
    /// Collects what a batch of awaited operations ran into and reports it once the batch is done.
    /// Waiting for every operation means a failure cannot be thrown where it happens, because that
    /// would leave the remaining ones unawaited and stranded in the pool.
    /// </summary>
    internal sealed class AwaitableFailures
    {
        private List<Exception> _failures;
        private bool _canceled;

        /// <summary>Records how one awaited operation ended.</summary>
        /// <param name="exception">The exception it ended with.</param>
        internal void Add(Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                _canceled = true;
                return;
            }

            _failures ??= new List<Exception>();
            _failures.Add(exception);
        }

        /// <summary>
        /// Throws what the batch ran into, if anything. A single failure is thrown as itself, so a
        /// caller can still catch its concrete type and the original stack trace survives. Several
        /// become one <see cref="AggregateException"/>, and a cancellation only surfaces when
        /// nothing failed outright.
        /// </summary>
        internal void Rethrow()
        {
            if (_failures == null
                && _canceled)
                throw new OperationCanceledException();

            if (_failures == null)
                return;

            if (_failures.Count == 1)
                ExceptionDispatchInfo.Capture(_failures[0]).Throw();

            throw new AggregateException(_failures);
        }
    }
}