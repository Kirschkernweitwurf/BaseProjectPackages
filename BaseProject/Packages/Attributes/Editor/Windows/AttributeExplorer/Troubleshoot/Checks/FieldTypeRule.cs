using System;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>One entry of the table that maps an attribute to the field types it can draw.</summary>
    internal readonly struct FieldTypeRule
    {
        /// <summary>The attribute the rule applies to.</summary>
        public readonly Type AttributeType;

        /// <summary>Returns whether the given field type is supported by the attribute.</summary>
        public readonly Func<Type, bool> Accepts;

        /// <summary>What the field type has to be, phrased for the message.</summary>
        public readonly string Requirement;

        /// <summary>Creates a rule.</summary>
        /// <param name="attributeType">The attribute the rule applies to.</param>
        /// <param name="accepts">Returns whether a field type is supported.</param>
        /// <param name="requirement">What the field type has to be.</param>
        public FieldTypeRule(Type attributeType, Func<Type, bool> accepts, string requirement)
        {
            AttributeType = attributeType;
            Accepts = accepts;
            Requirement = requirement;
        }
    }
}