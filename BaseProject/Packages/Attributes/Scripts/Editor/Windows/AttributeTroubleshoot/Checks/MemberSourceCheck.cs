using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Verifies that attributes reading their options from another member point at something usable.
    /// The picker drawers fall back to a plain field and a warning when the source is missing, so these
    /// mistakes are only visible while the affected object happens to be selected.
    /// </summary>
    internal sealed class MemberSourceCheck : IAttributeCheck
    {
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                VerifyDropdown(type, field, issues);
                VerifyProgressBar(type, field, issues);
                VerifySibling<AnimatorParamAttribute>(type, field, issues,
                    selector: attribute => attribute.AnimatorField, typeof(Animator));

                VerifySibling<AnimatorStateAttribute>(type, field, issues,
                    selector: attribute => attribute.AnimatorField, typeof(Animator));

                VerifySibling<MixerParameterAttribute>(type, field, issues,
                    selector: attribute => attribute.MixerField, typeof(AudioMixer));

                VerifySibling<AudioMixerGroupAttribute>(type, field, issues,
                    selector: attribute => attribute.MixerField, typeof(AudioMixer));

                VerifyShaderParam(type, field, issues);
            }
        }

        private static void VerifyDropdown(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            DropdownAttribute attribute = field.GetCustomAttribute<DropdownAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(DropdownAttribute);

            if (!CheckedMembers.Exists(owner, attribute.Member))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Member}' does not exist on {owner.Name}. The plain field is drawn instead.");

                return;
            }

            Type valueType = CheckedMembers.ValueTypeOf(owner, attribute.Member);
            if (!CheckedMembers.IsEnumerable(valueType))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Member}' is not an enumerable of options. The plain field is drawn instead.");
        }

        private static void VerifyProgressBar(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            ProgressBarAttribute attribute = field.GetCustomAttribute<ProgressBarAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.MaxMember))
                return;

            Type attributeType = typeof(ProgressBarAttribute);

            if (!CheckedMembers.Exists(owner, attribute.MaxMember))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.MaxMember}' does not exist on {owner.Name}.");

                return;
            }

            if (!CheckedMembers.IsNumeric(CheckedMembers.ValueTypeOf(owner, attribute.MaxMember)))
                AttributeIssues.Error(issues, field, attributeType, $"'{attribute.MaxMember}' is not numeric.");
        }

        // The sibling resolver used by these drawers matches on the exact field type, so a subclass or a
        // property of the right type does not qualify and the dropdown stays empty.
        private static void VerifySibling<T>(Type owner, FieldInfo field, List<AttributeIssue> issues,
            Func<T, string> selector, Type requiredType) where T : Attribute
        {
            T attribute = field.GetCustomAttribute<T>();
            if (attribute == null)
                return;

            string source = selector(attribute);
            if (string.IsNullOrEmpty(source))
                return;

            if (CheckedMembers.HasFieldOfExactType(owner, source, requiredType))
                return;

            string reason = CheckedMembers.Exists(owner, source)
                ? $"'{source}' is not a {requiredType.Name} field"
                : $"'{source}' does not exist on {owner.Name}";

            AttributeIssues.Error(issues, field, typeof(T), $"{reason}. The plain field is drawn instead.");
        }

        private static void VerifyShaderParam(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            ShaderParamAttribute attribute = field.GetCustomAttribute<ShaderParamAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(ShaderParamAttribute);
            FieldInfo source = ReflectionCache.GetField(owner, attribute.SourceField);

            if (source == null)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.SourceField}' does not exist on {owner.Name}.");

                return;
            }

            if (source.FieldType != typeof(Material)
                && source.FieldType != typeof(Shader)
                && !typeof(Renderer).IsAssignableFrom(source.FieldType))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.SourceField}' is not a Material, Renderer or Shader field.");
        }
    }
}