using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Verifies that attributes invoking a method point at one with a signature the renderer accepts.
    /// Buttons with parameters are skipped without a word, and a callback whose signature no longer
    /// matches simply stops firing, which is hard to notice from the inspector alone.
    /// </summary>
    internal sealed class MethodTargetCheck : IAttributeCheck
    {
        private const string ParameterlessMessage = "The method takes parameters, so no button is drawn.";

        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (MethodInfo method in ScannedMembers.DeclaredMethods(type))
                VerifyButtons(method, issues);

            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                VerifyInlineButton(type, field, issues);
                VerifyValidateInput(type, field, issues);
                VerifyOnValueChanged(type, field, issues);
                VerifyOnArraySizeChanged(type, field, issues);
            }
        }

        private static void VerifyButtons(MethodInfo method, List<AttributeIssue> issues)
        {
            if (method.GetParameters().Length == 0)
                return;

            if (method.GetCustomAttribute<ButtonAttribute>() != null)
                AttributeIssues.Error(issues, method, typeof(ButtonAttribute), ParameterlessMessage);

            if (method.GetCustomAttribute<HeaderButtonAttribute>() != null)
                AttributeIssues.Error(issues, method, typeof(HeaderButtonAttribute), ParameterlessMessage);
        }

        private static void VerifyInlineButton(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            InlineButtonAttribute attribute = field.GetCustomAttribute<InlineButtonAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(owner, attribute.Method);
            Type attributeType = typeof(InlineButtonAttribute);

            if (method == null)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' does not exist on {owner.Name}.");

                return;
            }

            if (method.GetParameters().Length > 0)
                AttributeIssues.Error(issues, field, attributeType, $"'{attribute.Method}' takes parameters.");
        }

        private static void VerifyValidateInput(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            ValidateInputAttribute attribute = field.GetCustomAttribute<ValidateInputAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(owner, attribute.MethodName);
            Type attributeType = typeof(ValidateInputAttribute);

            if (method == null)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.MethodName}' does not exist on {owner.Name}.");

                return;
            }

            if (method.ReturnType != typeof(bool))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.MethodName}' does not return bool, so the field is never validated.");

                return;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length > 1)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.MethodName}' takes more than one parameter.");

                return;
            }

            if (parameters.Length == 1 && !parameters[0].ParameterType.IsAssignableFrom(field.FieldType))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.MethodName}' does not accept a {field.FieldType.Name}.");
        }

        private static void VerifyOnValueChanged(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            OnValueChangedAttribute attribute = field.GetCustomAttribute<OnValueChangedAttribute>();
            if (attribute == null)
                return;

            MethodInfo method = ReflectionCache.GetMethod(owner, attribute.Method);
            Type attributeType = typeof(OnValueChangedAttribute);

            if (method == null)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' does not exist on {owner.Name}. The callback never fires.");

                return;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
                return;

            if (parameters.Length > 1)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' takes more than one parameter. The callback never fires.");

                return;
            }

            if (!parameters[0].ParameterType.IsAssignableFrom(field.FieldType))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' does not accept a {field.FieldType.Name}.");
        }

        private static void VerifyOnArraySizeChanged(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            OnArraySizeChangedAttribute attribute = field.GetCustomAttribute<OnArraySizeChangedAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(OnArraySizeChangedAttribute);

            if (!CheckedMembers.IsCollection(field.FieldType))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"{field.FieldType.Name} is not an array or list, so the callback never fires.");

                return;
            }

            MethodInfo method = ReflectionCache.GetMethod(owner, attribute.Method);
            if (method == null)
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' does not exist on {owner.Name}. The callback never fires.");

                return;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
                return;

            if (parameters.Length > 1 || parameters[0].ParameterType != typeof(int))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Method}' has to be parameterless or take a single int.");
        }
    }
}