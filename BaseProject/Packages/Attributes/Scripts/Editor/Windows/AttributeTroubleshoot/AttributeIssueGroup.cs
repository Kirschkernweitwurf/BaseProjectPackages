using System;
using System.Collections.Generic;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>All issues found on a single type, so the window can list them under one header.</summary>
    public sealed class AttributeIssueGroup
    {
        /// <summary>
        /// The type the issues were found on, or null for a fabricated sample group. Null groups cannot
        /// be opened in the code editor, because there is no script behind them.
        /// </summary>
        public Type Type { get; }

        /// <summary>Name shown in the group header.</summary>
        public string DisplayName { get; }

        /// <summary>The issues found on the type.</summary>
        public List<AttributeIssue> Issues { get; }

        /// <summary>Number of issues that stop an attribute from working.</summary>
        public int ErrorCount { get; }

        /// <summary>Creates a group for a real scanned type.</summary>
        /// <param name="type">The type the issues were found on.</param>
        /// <param name="issues">The issues found on the type.</param>
        public AttributeIssueGroup(Type type, List<AttributeIssue> issues)
            : this(type, type.Name, issues) { }

        /// <summary>Creates a group under a display name, for fabricated sample data.</summary>
        /// <param name="displayName">Name shown in the group header.</param>
        /// <param name="issues">The issues to list.</param>
        public AttributeIssueGroup(string displayName, List<AttributeIssue> issues)
            : this(null, displayName, issues) { }

        private AttributeIssueGroup(Type type, string displayName, List<AttributeIssue> issues)
        {
            Type = type;
            DisplayName = displayName;
            Issues = issues;

            int errors = 0;
            foreach (AttributeIssue issue in issues)
            {
                if (issue.Severity == EAttributeIssueSeverity.Error)
                    errors++;
            }

            ErrorCount = errors;
        }
    }
}
