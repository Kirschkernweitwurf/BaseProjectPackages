namespace UI.Confirmation
{
    /// <summary>
    /// Data container for confirmation dialog requests.
    /// </summary>
    public readonly struct ConfirmationRequest
    {
        public readonly string Message;
        public readonly string ConfirmText;
        public readonly string CancelText;

        public ConfirmationRequest(string message, string confirmText = null, string cancelText = null)
        {
            Message = message;
            ConfirmText = confirmText;
            CancelText = cancelText;
        }
    }
}