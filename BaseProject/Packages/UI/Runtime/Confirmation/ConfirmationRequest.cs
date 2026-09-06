namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Data container for confirmation dialog requests.
    /// <para>
    /// Both button labels are optional, and what an omitted one falls back to is a decision this type
    /// makes rather than the menu that draws it. A caller that builds its own confirmation UI gets the
    /// same reading of an empty label without repeating it.
    /// </para>
    /// </summary>
    public readonly struct ConfirmationRequest
    {
        /// <summary>The message shown to the player.</summary>
        public string Message { get; }

        /// <summary>The label of the confirm button. Empty falls back to the menu default.</summary>
        public string ConfirmText { get; }

        /// <summary>The label of the cancel button. Empty falls back to the menu default.</summary>
        public string CancelText { get; }

        /// <summary>
        /// Creates a request for the given message with optional button labels.
        /// </summary>
        /// <param name="message">The message shown to the player.</param>
        /// <param name="confirmText">Optional label of the confirm button.</param>
        /// <param name="cancelText">Optional label of the cancel button.</param>
        public ConfirmationRequest(string message, string confirmText = null, string cancelText = null)
        {
            Message = message;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }

        /// <summary>Reads the confirm label, or the given default when the request named none.</summary>
        /// <param name="fallback">The label to use when the request named none.</param>
        /// <returns>The label the confirm button shows.</returns>
        internal string ResolveConfirmText(string fallback) => Resolve(ConfirmText, fallback);

        /// <summary>Reads the cancel label, or the given default when the request named none.</summary>
        /// <param name="fallback">The label to use when the request named none.</param>
        /// <returns>The label the cancel button shows.</returns>
        internal string ResolveCancelText(string fallback) => Resolve(CancelText, fallback);

        // Empty and null read the same, because a caller that builds the label from a value it did not
        // have ends up with one just as often as a caller that left the argument out.
        private static string Resolve(string label, string fallback) => string.IsNullOrEmpty(label)
            ? fallback
            : label;
    }
}