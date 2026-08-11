using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of shader property names for <see cref="ShaderParamAttribute"/>. Reads the
    /// shader from a sibling Material, Renderer or Shader field. Stores the name on a string field and
    /// the id on an int field. While the source is missing or offers no matching properties, the plain
    /// field stays editable and a compact warning below explains what is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(ShaderParamAttribute))]
    public sealed class ShaderParamDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<ShaderParamAttribute>("a string or int");

        private string[] _names;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
            => Evaluate(property, (ShaderParamAttribute)attribute, out _names);

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            bool isString = property.propertyType == SerializedPropertyType.String;
            int current = CurrentIndex(property, _names, isString);
            int selected = LabeledField.Popup(rect, label, current, _names);

            if (selected < 0 || selected >= _names.Length || selected == current)
                return;

            if (isString)
                property.stringValue = _names[selected];
            else
                property.intValue = Shader.PropertyToID(_names[selected]);
        }

        private static string Evaluate(SerializedProperty property, ShaderParamAttribute attribute,
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

            names = CollectNames(shader, attribute.Type);
            if (names.Length > 0)
                return null;

            return attribute.HasFilter
                ? $"The shader has no {attribute.Type} properties."
                : "The shader has no properties.";
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

        private static string[] CollectNames(Shader shader, EShaderParamType filter)
        {
            List<string> names = new();
            int count = shader.GetPropertyCount();

            for (int i = 0; i < count; i++)
            {
                if (Matches(shader.GetPropertyType(i), filter))
                    names.Add(shader.GetPropertyName(i));
            }

            return names.ToArray();
        }

        private static bool Matches(ShaderPropertyType type, EShaderParamType filter)
        {
            switch (filter)
            {
                case EShaderParamType.Color:
                    return type == ShaderPropertyType.Color;
                case EShaderParamType.Vector:
                    return type == ShaderPropertyType.Vector;
                case EShaderParamType.Float:
                    return type == ShaderPropertyType.Float
                        || type == ShaderPropertyType.Range;
                case EShaderParamType.Texture:
                    return type == ShaderPropertyType.Texture;
                case EShaderParamType.Integer:
                    return type == ShaderPropertyType.Int;
                default:
                    return true;
            }
        }

        private static int CurrentIndex(SerializedProperty property, string[] names, bool isString)
        {
            if (isString)
                return Array.IndexOf(names, property.stringValue);

            int id = property.intValue;
            for (int i = 0; i < names.Length; i++)
            {
                if (Shader.PropertyToID(names[i]) == id)
                    return i;
            }

            return -1;
        }
    }
}