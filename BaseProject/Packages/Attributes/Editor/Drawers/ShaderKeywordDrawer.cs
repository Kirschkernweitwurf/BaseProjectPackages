using System;
using System.Reflection;
using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a dropdown of shader keyword names for <see cref="ShaderKeywordAttribute"/>. Reads the
    /// shader from a sibling Material, Renderer or Shader field, the same sources
    /// <see cref="ShaderParamAttribute"/> accepts. While the source is missing the plain field stays
    /// editable and a compact warning below explains what is wrong.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShaderKeywordAttribute))]
    internal sealed class ShaderKeywordDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<ShaderKeywordAttribute>("a string");

        private string[] _names;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String;

        protected override string Evaluate(SerializedProperty property)
            => Evaluate(property, (ShaderKeywordAttribute)attribute, out _names);

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            int current = Array.IndexOf(_names, property.stringValue);
            int selected = LabeledField.Popup(rect, label, current, _names);

            if (selected >= 0 && selected < _names.Length && selected != current)
                property.stringValue = _names[selected];
        }

        private static string Evaluate(SerializedProperty property, ShaderKeywordAttribute attribute,
            out string[] names)
        {
            names = null;

            if (!TryResolveSource(property, attribute.SourceField, out Object source))
                return $"Field '{attribute.SourceField}' was not found on this object.";

            if (source == null)
                return $"Field '{attribute.SourceField}' is not assigned.";

            Shader shader = ResolveShader(source);
            if (shader == null)
                return $"Field '{attribute.SourceField}' has no shader.";

            names = shader.keywordSpace.keywordNames;

            return names.Length > 0
                ? null
                : "The shader declares no keywords.";
        }

        // The source may be a Material, a Renderer or a Shader, so the field type is not known up front
        // and the generic sibling resolver, which matches on an exact type, cannot be used here.
        private static bool TryResolveSource(SerializedProperty property, string fieldName, out Object source)
        {
            source = null;

            Object target = property.serializedObject.targetObject;
            if (target == null || string.IsNullOrEmpty(fieldName))
                return false;

            FieldInfo field = ReflectionCache.GetField(target.GetType(), fieldName);
            if (field == null || !typeof(Object).IsAssignableFrom(field.FieldType))
                return false;

            source = field.GetValue(target) as Object;
            return true;
        }

        private static Shader ResolveShader(Object source)
        {
            switch (source)
            {
                case Shader shader:
                    return shader;
                case Material material:
                    return material.shader;
                case Renderer renderer:
                    return renderer.sharedMaterial == null
                        ? null
                        : renderer.sharedMaterial.shader;
                default:
                    return null;
            }
        }
    }
}