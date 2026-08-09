using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Verifies that conditional attributes point at members that actually resolve. An unresolved
    /// condition evaluates to true, so a mistyped or renamed member silently leaves the field always
    /// visible and always editable, which is the failure this check exists to surface.
    /// </summary>
    public sealed class ConditionMemberCheck : IAttributeCheck
    {
        private const string EmptyMessage = "No condition members given, so the attribute never applies.";

        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                ShowIfAttribute showIf = field.GetCustomAttribute<ShowIfAttribute>();
                if (showIf != null)
                    Verify(type, field, typeof(ShowIfAttribute), showIf.Members, issues);

                HideIfAttribute hideIf = field.GetCustomAttribute<HideIfAttribute>();
                if (hideIf != null)
                    Verify(type, field, typeof(HideIfAttribute), hideIf.Members, issues);

                EnableIfAttribute enableIf = field.GetCustomAttribute<EnableIfAttribute>();
                if (enableIf != null)
                    Verify(type, field, typeof(EnableIfAttribute), enableIf.Members, issues);

                DisableIfAttribute disableIf = field.GetCustomAttribute<DisableIfAttribute>();
                if (disableIf != null)
                    Verify(type, field, typeof(DisableIfAttribute), disableIf.Members, issues);

                RequiredIfAttribute requiredIf = field.GetCustomAttribute<RequiredIfAttribute>();
                if (requiredIf != null)
                    Verify(type, field, typeof(RequiredIfAttribute), requiredIf.Members, issues);

                VerifyEnum(type, field, issues);
            }
        }

        private static void Verify(Type owner, FieldInfo field, Type attributeType, string[] members,
            List<AttributeIssue> issues)
        {
            if (members == null || members.Length == 0)
            {
                AttributeIssues.Warning(issues, field, attributeType, EmptyMessage);
                return;
            }

            foreach (string member in members)
            {
                if (!CheckedMembers.Exists(owner, member))
                {
                    AttributeIssues.Error(issues, field, attributeType,
                        $"'{member}' does not exist on {owner.Name}. The condition always evaluates to true.");
                    continue;
                }

                if (!CheckedMembers.IsBool(owner, member))
                {
                    AttributeIssues.Error(issues, field, attributeType,
                        $"'{member}' is not a bool. The condition always evaluates to true.");
                }
            }
        }

        private static void VerifyEnum(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            ShowIfEnumAttribute attribute = field.GetCustomAttribute<ShowIfEnumAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(ShowIfEnumAttribute);

            if (!CheckedMembers.Exists(owner, attribute.Member))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Member}' does not exist on {owner.Name}.");
                return;
            }

            Type memberType = CheckedMembers.ValueTypeOf(owner, attribute.Member);
            if (memberType == null || !memberType.IsEnum)
            {
                AttributeIssues.Error(issues, field, attributeType, $"'{attribute.Member}' is not an enum.");
                return;
            }

            if (attribute.Values == null || attribute.Values.Length == 0)
            {
                AttributeIssues.Warning(issues, field, attributeType,
                    "No values given, so the field is never shown.");
                return;
            }

            foreach (object value in attribute.Values)
            {
                if (value == null || value.GetType() != memberType)
                {
                    AttributeIssues.Error(issues, field, attributeType,
                        $"A given value is not a {memberType.Name} and can never match.");
                    return;
                }
            }
        }
    }
}
