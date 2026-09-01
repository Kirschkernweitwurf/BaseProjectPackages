namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>A single misconfigured attribute usage found by a check.</summary>
    internal readonly struct AttributeIssue
    {
        /// <summary>Name of the field or method carrying the attribute.</summary>
        internal readonly string MemberName;

        /// <summary>Display name of the attribute, without the "Attribute" suffix.</summary>
        internal readonly string AttributeName;

        /// <summary>What is wrong and what the attribute does instead.</summary>
        internal readonly string Message;

        /// <summary>How badly the attribute behaves.</summary>
        internal readonly EAttributeIssueSeverity Severity;

        /// <summary>Creates an issue record.</summary>
        /// <param name="memberName">Name of the member carrying the attribute.</param>
        /// <param name="attributeName">Display name of the attribute.</param>
        /// <param name="message">What is wrong.</param>
        /// <param name="severity">How badly the attribute behaves.</param>
        public AttributeIssue(string memberName, string attributeName, string message,
            EAttributeIssueSeverity severity)
        {
            MemberName = memberName;
            AttributeName = attributeName;
            Message = message;
            Severity = severity;
        }
    }
}