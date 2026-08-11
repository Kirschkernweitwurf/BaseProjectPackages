using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Verifies that the attributes replacing Unity's array drawing sit on something that is actually an
    /// array. On anything else they are skipped without a word, so the field silently keeps the default
    /// drawing and the setting appears to have no effect.
    /// </summary>
    internal sealed class CollectionAttributeCheck : IAttributeCheck
    {
        private const string NotACollection = "is not an array or list, so the attribute is ignored.";

        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                bool isCollection = CheckedMembers.IsCollection(field.FieldType)
                    && field.FieldType != typeof(string);

                VerifyTarget<TableAttribute>(field, isCollection, issues);
                VerifyTarget<ListDrawerSettingsAttribute>(field, isCollection, issues);

                if (!isCollection)
                    continue;

                VerifyBothPresent(field, issues);
                VerifyLabelMember(field, issues);
                VerifyTableElement(field, issues);
            }
        }

        private static void VerifyTarget<T>(FieldInfo field, bool isCollection, List<AttributeIssue> issues)
            where T : Attribute
        {
            if (field.GetCustomAttribute<T>() == null || isCollection)
                return;

            AttributeIssues.Error(issues, field, typeof(T), $"{field.FieldType.Name} {NotACollection}");
        }

        // Both attributes take over the whole array, so only one of them can win.
        private static void VerifyBothPresent(FieldInfo field, List<AttributeIssue> issues)
        {
            if (field.GetCustomAttribute<TableAttribute>() == null
                || field.GetCustomAttribute<ListDrawerSettingsAttribute>() == null)
                return;

            AttributeIssues.Warning(issues, field, typeof(ListDrawerSettingsAttribute),
                "[Table] is on the same field and takes precedence, so these settings do nothing.");
        }

        private static void VerifyLabelMember(FieldInfo field, List<AttributeIssue> issues)
        {
            ListDrawerSettingsAttribute attribute = field.GetCustomAttribute<ListDrawerSettingsAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.LabelMember))
                return;

            Type element = CheckedMembers.ElementType(field.FieldType);
            if (element == null)
                return;

            if (ReflectionCache.GetField(element, attribute.LabelMember) != null)
                return;

            AttributeIssues.Error(issues, field, typeof(ListDrawerSettingsAttribute),
                $"'{attribute.LabelMember}' is not a serialized field of {element.Name}, so rows fall back "
                + "to their index. The label has to be a field, not a property.");
        }

        // A table of primitives has nothing to split into columns, so it would draw as an empty grid.
        private static void VerifyTableElement(FieldInfo field, List<AttributeIssue> issues)
        {
            if (field.GetCustomAttribute<TableAttribute>() == null)
                return;

            Type element = CheckedMembers.ElementType(field.FieldType);
            if (element == null)
                return;

            if (!element.IsPrimitive && element != typeof(string) && !element.IsEnum)
                return;

            AttributeIssues.Error(issues, field, typeof(TableAttribute),
                $"{element.Name} has no fields to turn into columns. Use [ListDrawerSettings] instead.");
        }
    }
}