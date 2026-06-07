#if UNITY_EDITOR
namespace Tools.Localization
{
    /// <summary>
    /// Result of a sync operation, indicating success or failure and an optional message.
    /// </summary>
    public readonly struct SyncResult
    {
        /// <summary>
        /// <c>true</c> if the sync operation succeeded, <c>false</c> if it failed.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// An optional message providing details about the sync result, such as an error message if it failed.
        /// </summary>
        public string Message { get; }

        private SyncResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        /// <summary>
        /// Creates a successful <see cref="SyncResult"/> with no message.
        /// </summary>
        /// <returns></returns>
        public static SyncResult Ok() => new(true, null);

        /// <summary>
        /// Creates a failed <see cref="SyncResult"/> with the provided error message.
        /// </summary>
        /// <param name="message">The error message describing why the sync operation failed.</param>
        /// <returns>A <see cref="SyncResult"/> indicating failure and containing the provided message.</returns>
        public static SyncResult Fail(string message) => new(false, message);
    }
}
#endif