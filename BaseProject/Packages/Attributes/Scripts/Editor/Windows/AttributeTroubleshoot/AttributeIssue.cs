using System;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>A single misconfigured attribute usage found by a check.</summary>
    public readonly struct AttributeIssue
    {
        /// <summary>The type that declares the member carrying the attribute.</summary>
        public readonly Type DeclaringType;

        /// <summary>Name of the field or method carrying the attribute.</summary>
        public readonly string MemberName;

        /// <summary>Display name of the attribute, without the "Attribute" suffix.</summary>
        public readonly string AttributeName;

        /// <summary>What is wrong and what the attribute does instead.</summary>
        public readonly string Message;

        /// <summary>How badly the attribute behaves.</summary>
        public readonly EAttributeIssueSeverity Severity;

        /// <summary>Creates an issue record.</summary>
        /// <param name="declaringType">The type that declares the member.</param>
        /// <param name="memberName">Name of the member carrying the attribute.</param>
        /// <param name="attributeName">Display name of the attribute.</param>
        /// <param name="message">What is wrong.</param>
        /// <param name="severity">How badly the attribute behaves.</param>
        public AttributeIssue(Type declaringType, string memberName, string attributeName, string message,
            EAttributeIssueSeverity severity)
        {
            DeclaringType = declaringType;
            MemberName = memberName;
            AttributeName = attributeName;
            Message = message;
            Severity = severity;
        }
    }
}
