using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Troubleshoot.Checks
{
    /// <summary>
    /// Verifies that each attribute sits on a field type its drawer can handle. A drawer on an
    /// unsupported type either falls back to the plain field or draws a usage hint in place of the
    /// value, and neither is obvious unless the object happens to be selected.
    /// </summary>
    internal sealed class FieldTypeCheck : IAttributeCheck
    {
        private const string ObjectReference = "an object reference";
        private const string StringOnly = "a string";
        private const string StringOrInt = "a string or int";

        private static readonly FieldTypeRule[] Rules =
        {
            new(typeof(TagAttribute), IsString, StringOnly),
            new(typeof(FilePathAttribute), IsString, StringOnly),
            new(typeof(FolderPathAttribute), IsString, StringOnly),
            new(typeof(ResourcesAssetAttribute), IsString, StringOnly),
            new(typeof(MaxLengthAttribute), IsString, StringOnly),
            new(typeof(SceneNameAttribute), IsStringOrInt, StringOrInt),
            new(typeof(LayerAttribute), IsStringOrInt, StringOrInt),
            new(typeof(SortingLayerAttribute), IsStringOrInt, StringOrInt),
            new(typeof(AnimatorParamAttribute), IsStringOrInt, StringOrInt),
            new(typeof(AnimatorStateAttribute), IsStringOrInt, StringOrInt),
            new(typeof(AudioMixerParameterAttribute), IsStringOrInt, StringOrInt),
            new(typeof(ShaderParamAttribute), IsStringOrInt, StringOrInt),
            new(typeof(PowerOfTwoAttribute), IsInteger, "an int"),
            new(typeof(PercentageAttribute), IsFloat, "a float"),
            new(typeof(ProgressBarAttribute), IsFloatOrInteger, "a float or int"),
            new(typeof(EnumToggleButtonsAttribute), IsEnum, "an enum"),
            new(typeof(MinMaxSliderAttribute), IsVector2, "a Vector2"),
            new(typeof(CurveRangeAttribute), IsCurve, "an AnimationCurve"),
            new(typeof(RequiredAttribute), IsObject, ObjectReference),
            new(typeof(RequiredIfAttribute), IsObject, ObjectReference),
            new(typeof(MustImplementAttribute), IsObject, ObjectReference),
            new(typeof(AssetOnlyAttribute), IsObject, ObjectReference),
            new(typeof(SceneObjectOnlyAttribute), IsObject, ObjectReference),
            new(typeof(ShowAssetPreviewAttribute), IsObject, ObjectReference),
            new(typeof(OpenAssetAttribute), IsObject, ObjectReference),
            new(typeof(ExpandableAttribute), IsObject, ObjectReference),
            new(typeof(ComponentPickerAttribute), IsComponent, "a Component reference"),
            new(typeof(AudioMixerGroupAttribute), IsMixerGroup, "an AudioMixerGroup reference")
        };

        public void Inspect(Type type, List<AttributeIssue> issues)
        {
            foreach (FieldInfo field in ScannedMembers.DeclaredFields(type))
            {
                // Attributes on an array or list apply to each element, so the element type decides.
                Type fieldType = CheckedMembers.ElementType(field.FieldType);
                if (fieldType == null)
                    continue;

                foreach (FieldTypeRule rule in Rules)
                {
                    if (field.GetCustomAttribute(rule.AttributeType) == null || rule.Accepts(fieldType))
                        continue;

                    string usage = AttributeNames.Usage(rule.AttributeType, rule.Requirement);
                    AttributeIssues.Error(issues, field, rule.AttributeType,
                        $"{fieldType.Name} is not supported. {usage}");
                }
            }
        }

        private static bool IsCurve(Type type) => type == typeof(AnimationCurve);

        private static bool IsComponent(Type type) => typeof(Component).IsAssignableFrom(type) || type.IsInterface;

        private static bool IsEnum(Type type) => type.IsEnum;

        private static bool IsFloat(Type type) => type == typeof(float) || type == typeof(double);

        private static bool IsFloatOrInteger(Type type) => IsFloat(type) || IsInteger(type);

        private static bool IsInteger(Type type) => type == typeof(int);

        private static bool IsMixerGroup(Type type) => typeof(AudioMixerGroup).IsAssignableFrom(type);

        private static bool IsObject(Type type) => typeof(Object).IsAssignableFrom(type);

        private static bool IsString(Type type) => type == typeof(string);

        private static bool IsStringOrInt(Type type) => IsString(type) || IsInteger(type);

        private static bool IsVector2(Type type) => type == typeof(Vector2) || type == typeof(Vector2Int);
    }
}