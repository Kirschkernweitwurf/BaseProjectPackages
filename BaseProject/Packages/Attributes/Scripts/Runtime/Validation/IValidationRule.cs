using System.Reflection;

namespace Base.AttributePackage
{
    /// <summary>
    /// A single validation check for a serialized field. Implement this interface anywhere and the
    /// scanner, the runtime validator and the overview window pick it up automatically, no manual
    /// registration required.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Returns true and a short reason when the field carries this rule's attribute and its current
        /// value is invalid. Returns false when the rule does not apply or the value is valid.
        /// </summary>
        bool IsViolated(FieldInfo field, object instance, out string reason);
    }
}