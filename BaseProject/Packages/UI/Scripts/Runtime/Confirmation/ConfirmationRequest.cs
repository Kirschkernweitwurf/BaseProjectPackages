namespace Base.UIPackage.Confirmation
{
    /// <summary>
    /// Data container for confirmation dialog requests.
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
    }
}