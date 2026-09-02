using System;
using UnityEngine;

namespace Base.AttributesPackage
{
    /// <summary>
    /// Fills a component reference from the hierarchy and reports it when nothing was found. The
    /// auto-assign and the requirement in one attribute, since on a mandatory sibling reference they are
    /// always written together.
    /// </summary>
    /// <remarks>
    /// Searches this GameObject by default. Set <see cref="InParents"/> or <see cref="InChildren"/> to
    /// widen it, and clear <see cref="IncludeSelf"/> to exclude this object from either.
    /// <para>
    /// Works on an array or list too, in which case every match is collected rather than the first.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredGetAttribute : PropertyAttribute
    {
        /// <summary>Reason text used when no custom message is set. Shared by the rule and the handler.</summary>
        public const string DefaultReason = "was not found in the hierarchy";

        /// <summary>Whether ancestors are searched.</summary>
        public bool InParents { get; set; }

        /// <summary>Whether descendants are searched.</summary>
        public bool InChildren { get; set; }

        /// <summary>Whether this GameObject counts as part of the search.</summary>
        public bool IncludeSelf { get; set; } = true;

        /// <summary>Whether inactive objects are searched too.</summary>
        public bool IncludeInactive { get; set; } = true;

        /// <summary>Optional custom message. Null uses a default message.</summary>
        public string Message { get; set; }
    }
}