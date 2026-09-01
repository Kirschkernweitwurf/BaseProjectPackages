using System;
using System.Threading;
using UnityEngine;

namespace Base.UtilityPackage.Async
{
    /// <summary>
    /// A deadline for an awaitable operation. Cancels <see cref="Token"/> once the given number of
    /// seconds has passed and reports through <see cref="HasExpired"/> that it, and not the caller,
    /// is the reason the operation was canceled.
    /// </summary>
    /// <remarks>
    /// The countdown starts with the instance and runs on the player loop, so it only advances
    /// while the game does. Disposing stops it, which is why every user has to dispose even when
    /// the operation finished long before the deadline.
    /// </remarks>
    internal sealed class AwaitableTimeout : IDisposable
    {
        /// <summary>The token the operation has to observe. Canceled once the deadline passes.</summary>
        internal CancellationToken Token => _linked.Token;

        /// <summary>True once the deadline passed, which is what tells a timeout from a caller's cancel.</summary>
        internal bool HasExpired { get; private set; }

        private readonly CancellationTokenSource _linked;
        private readonly float _seconds;

        /// <summary>Starts the countdown.</summary>
        /// <param name="seconds">How long the operation may take.</param>
        /// <param name="cancellationToken">The caller's token, which cancels the operation as well.</param>
        internal AwaitableTimeout(float seconds, CancellationToken cancellationToken)
        {
            _seconds = seconds;
            _linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Expire().Forget();
        }

        /// <summary>Stops the countdown and releases the linked token source.</summary>
        public void Dispose()
        {
            // Canceling is what wakes the waiting countdown, so it ends here instead of holding on
            // until a deadline nobody is watching for anymore.
            _linked.Cancel();
            _linked.Dispose();
        }

        private async Awaitable Expire()
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(_seconds, _linked.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            HasExpired = true;
            _linked.Cancel();
        }
    }
}