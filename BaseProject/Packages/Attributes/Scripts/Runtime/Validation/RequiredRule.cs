using System.Reflection;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>Fails when a <see cref="RequiredAttribute"/> object reference is null.</summary>
    public sealed class RequiredRule : IValidationRule
    {
        public bool IsViolated(FieldInfo field, object instance, out string reason)
        {
            reason = null;

            RequiredAttribute attribute = field.GetCustomAttribute<RequiredAttribute>(true);
            if (attribute == null)
                return false;

            if (!typeof(Object).IsAssignableFrom(field.FieldType))
                return false;

            if (field.GetValue(instance) as Object != null)
                return false;

            reason = attribute.Message ?? "is required";
            return true;
        }
    }
}