using System.Reflection;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>
    /// Fails when a <see cref="RequiredIfAttribute"/> object reference is null while its condition
    /// holds, so conditional requirements appear in the overview window alongside plain ones.
    /// </summary>
    public sealed class RequiredIfRule : IValidationRule
    {
        /// <inheritdoc/>
        public bool IsViolated(FieldInfo field, object instance, out string reason)
        {
            reason = null;

            RequiredIfAttribute attribute = field.GetCustomAttribute<RequiredIfAttribute>(true);
            if (attribute == null)
                return false;

            if (!typeof(Object).IsAssignableFrom(field.FieldType))
                return false;

            if (!ConditionMembers.Evaluate(instance, attribute.Mode, attribute.Members))
                return false;

            if (field.GetValue(instance) as Object != null)
                return false;

            reason = attribute.Message ?? RequiredIfAttribute.DefaultReason;
            return true;
        }
    }
}
