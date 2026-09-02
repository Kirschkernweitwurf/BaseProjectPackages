namespace Base.AttributesPackage
{
    /// <summary>
    /// The outcome of a <see cref="ValidateInputAttribute"/> method, carrying a severity and its own
    /// message rather than a bare bool.
    /// </summary>
    /// <remarks>
    /// A bool can only say pass or fail, and only ever with the one message baked into the attribute. A
    /// validator usually knows more than that: an empty reference and an unreadable texture are both
    /// failures, but they are not the same failure and they do not deserve the same words. Returning a
    /// result lets the method say which, and lets it downgrade to a warning where the value still works.
    /// <para>
    /// A validator returning bool keeps working. Both shapes are accepted.
    /// </para>
    /// </remarks>
    public readonly struct ValidationResult
    {
        /// <summary>A passing result with nothing to say.</summary>
        public static readonly ValidationResult Valid = new(EValidationSeverity.Valid, null);

        /// <summary>How badly the value failed.</summary>
        public EValidationSeverity Severity { get; }

        /// <summary>What to tell the user, or null to fall back to the attribute's message.</summary>
        public string Message { get; }

        /// <summary>True when nothing should be drawn for this result.</summary>
        public bool IsValid => Severity == EValidationSeverity.Valid;

        private ValidationResult(EValidationSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        /// <summary>Creates a warning: the value works, but probably not as intended.</summary>
        /// <param name="message">What to tell the user.</param>
        /// <returns>The result.</returns>
        public static ValidationResult Warning(string message) => new(EValidationSeverity.Warning, message);

        /// <summary>Creates an error: the value is wrong and something will break.</summary>
        /// <param name="message">What to tell the user.</param>
        /// <returns>The result.</returns>
        public static ValidationResult Error(string message) => new(EValidationSeverity.Error, message);
    }
}