namespace Systems.CheatConsole
{
    /// <summary>
    /// Represents the result of executing a cheat command.
    /// </summary>
    public sealed class CheatConsoleResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheatConsoleResult"/> class.
        /// </summary>
        /// <param name="message">The associated message (info, error, etc.).</param>
        /// <param name="messageType">The type of message to display in the console.</param>
        public CheatConsoleResult(string message, CheatConsoleMessageType messageType)
        {
            Message = message;
            MessageType = messageType;
        }

        /// <summary>
        /// Gets the descriptive message resulting from execution.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the message type to display.
        /// </summary>
        public CheatConsoleMessageType MessageType { get; }
    }
}