using System;
using System.Collections;
using System.Reflection;
using Object = UnityEngine.Object;

namespace Base.AttributePackage
{
    /// <summary>Fails when a <see cref="NotNullOrEmptyAttribute"/> value is null or empty.</summary>
    public sealed class NotNullOrEmptyRule : IValidationRule
    {
        public bool IsViolated(FieldInfo field, object instance, out string reason)
        {
            reason = null;

            NotNullOrEmptyAttribute attribute = field.GetCustomAttribute<NotNullOrEmptyAttribute>(true);
            if (attribute == null)
                return false;

            if (!IsEmpty(field.FieldType, field.GetValue(instance)))
                return false;

            reason = attribute.Message ?? "must not be empty";
            return true;
        }

        private static bool IsEmpty(Type type, object value)
        {
            if (type == typeof(string))
                return string.IsNullOrEmpty((string)value);

            if (value is IList list)
                return list.Count == 0;

            if (typeof(Object).IsAssignableFrom(type))
                return value as Object == null;

            return value == null;
        }
    }
}