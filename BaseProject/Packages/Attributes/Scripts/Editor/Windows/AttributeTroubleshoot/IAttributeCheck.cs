using System;
using System.Collections.Generic;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>
    /// A single family of attribute misconfigurations. Implement this interface anywhere and the
    /// troubleshoot window picks it up automatically, no manual registration required. Checks are
    /// stateless, since one instance is shared across every scanned type.
    /// </summary>
    internal interface IAttributeCheck
    {
        /// <summary>
        /// Inspects the members declared directly on the given type and appends every problem found.
        /// Inherited members are inspected when their own declaring type is scanned, so nothing is
        /// reported twice.
        /// </summary>
        /// <param name="type">The type to inspect.</param>
        /// <param name="issues">The list every finding is appended to.</param>
        void Inspect(Type type, List<AttributeIssue> issues);
    }
}