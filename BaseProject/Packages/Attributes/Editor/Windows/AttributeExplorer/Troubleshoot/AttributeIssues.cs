using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Core;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Builds issue records so every check phrases its findings the same way and derives the attribute
    /// name from its type instead of a literal.
    /// </summary>
    internal static class AttributeIssues
    {
        /// <summary>Records a problem that stops the attribute from working at all.</summary>
        /// <param name="issues">The list the finding is appended to.</param>
        /// <param name="member">The member carrying the attribute.</param>
        /// <param name="attributeType">The attribute type that is misconfigured.</param>
        /// <param name="message">What is wrong.</param>
        public static void Error(List<AttributeIssue> issues, MemberInfo member, Type attributeType, string message)
            => Add(issues, member, attributeType, message, EAttributeIssueSeverity.Error);

        /// <summary>Records a problem that changes the attribute's behavior without disabling it.</summary>
        /// <param name="issues">The list the finding is appended to.</param>
        /// <param name="member">The member carrying the attribute.</param>
        /// <param name="attributeType">The attribute type that is misconfigured.</param>
        /// <param name="message">What is wrong.</param>
        public static void Warning(List<AttributeIssue> issues, MemberInfo member, Type attributeType, string message)
            => Add(issues, member, attributeType, message, EAttributeIssueSeverity.Warning);

        private static void Add(List<AttributeIssue> issues, MemberInfo member, Type attributeType, string message,
            EAttributeIssueSeverity severity) => issues.Add(new AttributeIssue(member.Name,
            AttributeNames.Display(attributeType), message, severity));
    }
}