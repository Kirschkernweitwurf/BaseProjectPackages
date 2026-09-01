using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>
    /// Checks the attributes that read their text or their bounds from another member, plus the layout
    /// attributes whose target type decides whether they can work at all.
    /// </summary>
    /// <remarks>
    /// A member reference that no longer resolves falls back to the literal, which means a title reads
    /// as <c>$Caption</c> and a slider silently keeps a bound of zero. Both are quiet failures, which is
    /// what this window exists to make loud.
    /// </remarks>
    internal sealed class LayoutAttributeCheck : IAttributeCheck
    {
        private const string LiteralFallback = "so the attribute falls back to showing the reference itself.";

        /// <inheritdoc/>
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                VerifyText<TitleAttribute>(type, field, issues, selector: attribute => attribute.Title);
                VerifyText<InfoBoxAttribute>(type, field, issues, selector: attribute => attribute.Message);
                VerifyText<LabelAttribute>(type, field, issues, selector: attribute => attribute.Text);
                VerifyText<RequiredAttribute>(type, field, issues, selector: attribute => attribute.Message);
                VerifyText<RequiredIfAttribute>(type, field, issues, selector: attribute => attribute.Message);
                VerifyText<NotNullOrEmptyAttribute>(type, field, issues, selector: attribute => attribute.Message);

                VerifyFix<RequiredAttribute>(type, field, issues, selector: attribute => attribute.FixAction);
                VerifyFix<ValidateInputAttribute>(type, field, issues, selector: attribute => attribute.FixAction);

                VerifySlider(type, field, issues);
                VerifyMinMaxSlider(type, field, issues);
                VerifyInline(field, issues);
                VerifyHorizontal(field, issues);
                VerifyRequiredGet(field, issues);
                VerifyPreview(field, issues);
                VerifySuffix(field, issues);
            }
        }

        // A literal is always fine. Only a member reference can be wrong, and only by not existing.
        private static void VerifyText<T>(Type owner, FieldInfo field, List<AttributeIssue> issues,
            Func<T, string> selector) where T : Attribute
        {
            T attribute = field.GetCustomAttribute<T>();
            if (attribute == null)
                return;

            string value = selector(attribute);
            if (!ValueResolver.IsMemberReference(value))
                return;

            string member = ValueResolver.MemberName(value);
            if (CheckedMembers.Exists(owner, member))
                return;

            AttributeIssues.Error(issues, field, typeof(T),
                $"'{member}' does not exist on {owner.Name}, {LiteralFallback}");
        }

        private static void VerifyFix<T>(Type owner, FieldInfo field, List<AttributeIssue> issues,
            Func<T, string> selector) where T : Attribute
        {
            T attribute = field.GetCustomAttribute<T>();
            if (attribute == null)
                return;

            string method = selector(attribute);
            if (string.IsNullOrEmpty(method))
                return;

            MethodInfo found = ReflectionCache.GetMethod(owner, method);

            if (found == null)
            {
                AttributeIssues.Error(issues, field, typeof(T),
                    $"'{method}' does not exist on {owner.Name}, so no fix button is drawn.");

                return;
            }

            if (found.GetParameters().Length > 0)
                AttributeIssues.Error(issues, field, typeof(T),
                    $"'{method}' takes parameters, so no fix button is drawn.");
        }

        private static void VerifySlider(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            SliderAttribute attribute = field.GetCustomAttribute<SliderAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(SliderAttribute);

            if (field.FieldType != typeof(float) && field.FieldType != typeof(int))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"{field.FieldType.Name} is not a float or an int.");

                return;
            }

            VerifyBound(owner, field, attributeType, attribute.MinMember, issues);
            VerifyBound(owner, field, attributeType, attribute.MaxMember, issues);
            VerifyRange(owner, field, attributeType, attribute.RangeMember, issues);
        }

        private static void VerifyMinMaxSlider(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            MinMaxSliderAttribute attribute = field.GetCustomAttribute<MinMaxSliderAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(MinMaxSliderAttribute);

            VerifyBound(owner, field, attributeType, attribute.MinMember, issues);
            VerifyBound(owner, field, attributeType, attribute.MaxMember, issues);
            VerifyRange(owner, field, attributeType, attribute.RangeMember, issues);
        }

        private static void VerifyBound(Type owner, FieldInfo field, Type attributeType, string member,
            List<AttributeIssue> issues)
        {
            if (string.IsNullOrEmpty(member))
                return;

            if (!CheckedMembers.Exists(owner, member))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{member}' does not exist on {owner.Name}, so that bound stays at zero.");

                return;
            }

            if (!CheckedMembers.IsNumeric(CheckedMembers.ValueTypeOf(owner, member)))
                AttributeIssues.Error(issues, field, attributeType, $"'{member}' is not numeric.");
        }

        private static void VerifyRange(Type owner, FieldInfo field, Type attributeType, string member,
            List<AttributeIssue> issues)
        {
            if (string.IsNullOrEmpty(member))
                return;

            if (!CheckedMembers.Exists(owner, member))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{member}' does not exist on {owner.Name}, so both bounds stay at zero.");

                return;
            }

            Type valueType = CheckedMembers.ValueTypeOf(owner, member);

            if (valueType != typeof(Vector2) && valueType != typeof(Vector2Int))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{member}' is not a Vector2, so it cannot carry both bounds.");
        }

        // The renderer falls back to a foldout rather than drawing something a row cannot hold, so this
        // is a warning about a setting having no effect rather than an error.
        private static void VerifyInline(FieldInfo field, List<AttributeIssue> issues)
        {
            if (field.GetCustomAttribute<InlinePropertyAttribute>() == null)
                return;

            Type type = field.FieldType;

            if (type.IsPrimitive
                || type == typeof(string)
                || type.IsEnum
                || typeof(Object).IsAssignableFrom(type))
            {
                AttributeIssues.Error(issues, field, typeof(InlinePropertyAttribute),
                    $"{type.Name} has no children to inline.");

                return;
            }

            if (CheckedMembers.IsCollection(type))
                AttributeIssues.Error(issues, field, typeof(InlinePropertyAttribute),
                    "A collection cannot be drawn on one row. Use [ListDrawerSettings] or [Table].");
        }

        private static void VerifyHorizontal(FieldInfo field, List<AttributeIssue> issues)
        {
            HorizontalAttribute attribute = field.GetCustomAttribute<HorizontalAttribute>();
            if (attribute == null)
                return;

            if (string.IsNullOrEmpty(attribute.Group))
            {
                AttributeIssues.Error(issues, field, typeof(HorizontalAttribute),
                    "The row needs a name, since that is what decides which fields share it.");

                return;
            }

            if (attribute.Weight <= 0f)
                AttributeIssues.Error(issues, field, typeof(HorizontalAttribute),
                    "A weight of zero or less would give the field no width.");
        }

        private static void VerifyRequiredGet(FieldInfo field, List<AttributeIssue> issues)
        {
            RequiredGetAttribute attribute = field.GetCustomAttribute<RequiredGetAttribute>();
            if (attribute == null)
                return;

            Type element = CheckedMembers.ElementType(field.FieldType);
            Type attributeType = typeof(RequiredGetAttribute);

            if (element == null || !typeof(Component).IsAssignableFrom(element) && !element.IsInterface)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"{field.FieldType.Name} is not a component type, so the search can never fill it.");

                return;
            }

            if (!attribute.IncludeSelf && !attribute.InParents && !attribute.InChildren)
                AttributeIssues.Error(issues, field, attributeType,
                    "Self is excluded and neither parents nor children are searched, so nothing is left "
                    + "to look through.");
        }

        private static void VerifyPreview(FieldInfo field, List<AttributeIssue> issues)
        {
            PreviewObjectAttribute attribute = field.GetCustomAttribute<PreviewObjectAttribute>();
            if (attribute == null)
                return;

            if (!typeof(Object).IsAssignableFrom(CheckedMembers.ElementType(field.FieldType)))
            {
                AttributeIssues.Error(issues, field, typeof(PreviewObjectAttribute),
                    $"{field.FieldType.Name} is not an object reference, so there is nothing to preview.");

                return;
            }

            if (attribute.Height <= 0f)
                AttributeIssues.Error(issues, field, typeof(PreviewObjectAttribute),
                    "A height of zero or less leaves the preview invisible.");
        }

        private static void VerifySuffix(FieldInfo field, List<AttributeIssue> issues)
        {
            SuffixAttribute attribute = field.GetCustomAttribute<SuffixAttribute>();
            if (attribute == null)
                return;

            if (string.IsNullOrEmpty(attribute.Text))
                AttributeIssues.Warning(issues, field, typeof(SuffixAttribute),
                    "The suffix text is empty, so nothing is drawn.");
        }
    }
}