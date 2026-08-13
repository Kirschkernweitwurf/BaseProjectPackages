using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Checks
{
    /// <summary>
    /// Checks the attributes added alongside the collection and handle work: the size limits, the prefix
    /// toggle, the palette, the context menu, the header controls and the searching auto-getters. Each
    /// of these fails by doing nothing, which is the failure this window exists to make visible.
    /// </summary>
    internal sealed class WidgetAttributeCheck : IAttributeCheck
    {
        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                VerifyArraySize(field, issues);
                VerifyPalette(type, field, issues);
                VerifyAutoGetters(field, issues);
            }

            foreach (MethodInfo method in ScannedMembers.DeclaredMethods(type))
                VerifyHeaderMembers(method, issues);
        }

        private static void VerifyArraySize(FieldInfo field, List<AttributeIssue> issues)
        {
            ArraySizeAttribute attribute = field.GetCustomAttribute<ArraySizeAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(ArraySizeAttribute);

            if (!CheckedMembers.IsCollection(field.FieldType) || field.FieldType == typeof(string))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"{field.FieldType.Name} is not an array or list, so there is no size to limit.");

                return;
            }

            if (attribute.Size < 0 && attribute.Min < 0 && attribute.Max < 0)
            {
                AttributeIssues.Warning(issues, field, attributeType,
                    "Neither a size nor a range was given, so nothing is limited.");

                return;
            }

            if (attribute.Min >= 0 && attribute.Max >= 0 && attribute.Min > attribute.Max)
                AttributeIssues.Error(issues, field, attributeType,
                    $"Min {attribute.Min} is above Max {attribute.Max}, so no count can satisfy both.");
        }

        private static void VerifyPalette(Type owner, FieldInfo field, List<AttributeIssue> issues)
        {
            ColorPaletteAttribute attribute = field.GetCustomAttribute<ColorPaletteAttribute>();
            if (attribute == null)
                return;

            Type attributeType = typeof(ColorPaletteAttribute);

            if (field.FieldType != typeof(Color))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"{field.FieldType.Name} is not a Color.");

                return;
            }

            if (!CheckedMembers.Exists(owner, attribute.Member))
            {
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Member}' does not exist on {owner.Name}.");

                return;
            }

            if (!CheckedMembers.IsEnumerable(CheckedMembers.ValueTypeOf(owner, attribute.Member)))
                AttributeIssues.Error(issues, field, attributeType,
                    $"'{attribute.Member}' is not an enumerable of colors.");
        }

        // These fill an object reference, so anything else has nowhere for the result to go.
        private static void VerifyAutoGetters(FieldInfo field, List<AttributeIssue> issues)
        {
            Type fieldType = CheckedMembers.ElementType(field.FieldType);
            if (fieldType == null)
                return;

            VerifyAssignable<GetScriptableObjectAttribute>(field, fieldType, typeof(ScriptableObject), issues);
            VerifyAssignable<GetInSceneAttribute>(field, fieldType, typeof(Component), issues);

            GetPrefabWithComponentAttribute prefab = field.GetCustomAttribute<GetPrefabWithComponentAttribute>();
            if (prefab == null)
                return;

            if (fieldType == typeof(GameObject) && prefab.ComponentType == null)
            {
                AttributeIssues.Error(issues, field, typeof(GetPrefabWithComponentAttribute),
                    "A GameObject field gives the search nothing to look for. Name the component type in "
                    + "the attribute.");

                return;
            }

            Type required = prefab.ComponentType ?? fieldType;

            if (!typeof(Component).IsAssignableFrom(required) && !required.IsInterface)
                AttributeIssues.Error(issues, field, typeof(GetPrefabWithComponentAttribute),
                    $"{required.Name} is not a component type, so no prefab can carry it.");
        }

        private static void VerifyAssignable<T>(FieldInfo field, Type fieldType, Type required,
            List<AttributeIssue> issues) where T : Attribute
        {
            if (field.GetCustomAttribute<T>() == null || required.IsAssignableFrom(fieldType))
                return;

            AttributeIssues.Error(issues, field, typeof(T),
                $"{fieldType.Name} is not a {required.Name}, so the field is never filled.");
        }

        private static void VerifyHeaderMembers(MethodInfo method, List<AttributeIssue> issues)
        {
            if (method.GetCustomAttribute<HeaderLabelAttribute>() != null)
                if (method.GetParameters().Length > 0 || method.ReturnType == typeof(void))
                    AttributeIssues.Error(issues, method, typeof(HeaderLabelAttribute),
                        "A header label needs a parameterless member that returns a value to show.");

            HeaderDrawAttribute draw = method.GetCustomAttribute<HeaderDrawAttribute>();
            if (draw == null)
                return;

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(Rect))
                AttributeIssues.Error(issues, method, typeof(HeaderDrawAttribute),
                    "A header draw method has to take a single Rect.");
        }
    }
}