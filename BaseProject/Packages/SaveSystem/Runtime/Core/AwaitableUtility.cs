using UnityEngine;

namespace Base.SaveSystemPackage.Core
{
    /// <summary>
    /// Helpers for code that has to satisfy an <see cref="Awaitable"/> signature without doing any
    /// asynchronous work. Returning an already completed awaitable keeps such an implementation honest:
    /// the caller resumes straight away instead of losing a frame to a fake await.
    /// </summary>
    public static class AwaitableUtility
    {
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
    }
}