using System;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>
    /// Fails when a <see cref="MustImplementAttribute"/> reference holds an object that does not
    /// satisfy the required types, so type violations show up in the overview window.
    /// </summary>
    internal sealed class MustImplementRule : IValidationRule
    {
        /// <inheritdoc/>
        public bool IsViolated(FieldInfo field, object instance, out string reason)
        {
            reason = null;

            MustImplementAttribute attribute = field.GetCustomAttribute<MustImplementAttribute>(true);
            if (attribute == null || attribute.Types == null || attribute.Types.Length == 0)
                return false;

            if (!typeof(Object).IsAssignableFrom(field.FieldType))
                return false;

            // A null reference is the job of [Required], not of this rule.
            if (field.GetValue(instance) is not Object value || value == null)
                return false;

            foreach (Type required in attribute.Types)
            {
                if (required == null || required.IsInstanceOfType(value))
                    continue;

                reason = $"does not implement {required.Name}";
                return true;
            }

            return false;
        }
    }
}