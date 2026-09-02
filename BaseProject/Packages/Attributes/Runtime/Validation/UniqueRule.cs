using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributesPackage
{
    /// <summary>Fails when a <see cref="UniqueAttribute"/> list contains the same entry twice.</summary>
    internal sealed class UniqueRule : IValidationRule
    {
        // Reused across calls. The scanner runs one rule at a time, so no state survives a check.
        private static readonly List<string> Groups = new();

        /// <inheritdoc/>
        public bool IsViolated(FieldInfo field, object instance, out string reason)
        {
            reason = null;

            UniqueAttribute attribute = field.GetCustomAttribute<UniqueAttribute>(true);
            if (attribute == null)
                return false;

            if (field.GetValue(instance) is not IList list)
                return false;

            DuplicateFinder.Collect(list, Groups);
            if (Groups.Count == 0)
                return false;

            reason = attribute.Message ?? DuplicateFinder.Describe(Groups);
            return true;
        }
    }
}