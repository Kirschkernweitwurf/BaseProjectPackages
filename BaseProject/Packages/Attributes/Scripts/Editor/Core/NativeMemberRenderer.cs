using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws read-only values for members marked with <see cref="ShowNonSerializedAttribute"/>
    /// and <see cref="ShowNativePropertyAttribute"/>. The annotated members are collected once per
    /// type and cached, so repaints do not scan any members.
    /// </summary>
    public static class NativeMemberRenderer
    {
        private static readonly Dictionary<Type, FieldInfo[]> Fields = new();
        private static readonly Dictionary<Type, PropertyInfo[]> Properties = new();

        /// <summary>Draws all native members for the edited object.</summary>
        public static void Draw(UnityEditor.Editor editor)
        {
            Object target = editor.target;
            Type type = target.GetType();

            foreach (FieldInfo field in GetFields(type))
                DrawValue(ObjectNames.NicifyVariableName(field.Name), field.GetValue(target));

            foreach (PropertyInfo property in GetProperties(type))
            {
                object value;
                try
                {
                    value = property.GetValue(target, null);
                }
                catch (Exception exception)
                {
                    value = exception.Message;
                }

                DrawValue(ObjectNames.NicifyVariableName(property.Name), value);
            }
        }

        private static FieldInfo[] GetFields(Type type)
        {
            if (Fields.TryGetValue(type, out FieldInfo[] cached))
                return cached;

            List<FieldInfo> fields = new();
            foreach (FieldInfo field in ReflectionCache.AllFields(type))
            {
                if (field.GetCustomAttribute<ShowNonSerializedAttribute>() != null)
                    fields.Add(field);
            }

            FieldInfo[] result = fields.ToArray();
            Fields[type] = result;
            return result;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            if (Properties.TryGetValue(type, out PropertyInfo[] cached))
                return cached;

            List<PropertyInfo> properties = new();
            foreach (PropertyInfo property in ReflectionCache.AllProperties(type))
            {
                if (property.CanRead && property.GetCustomAttribute<ShowNativePropertyAttribute>() != null)
                    properties.Add(property);
            }

            PropertyInfo[] result = properties.ToArray();
            Properties[type] = result;
            return result;
        }

        private static void DrawValue(string label, object value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                switch (value)
                {
                    case null:
                        EditorGUILayout.TextField(label, "null");
                        break;
                    case int intValue:
                        EditorGUILayout.IntField(label, intValue);
                        break;
                    case float floatValue:
                        EditorGUILayout.FloatField(label, floatValue);
                        break;
                    case bool boolValue:
                        EditorGUILayout.Toggle(label, boolValue);
                        break;
                    case string stringValue:
                        EditorGUILayout.TextField(label, stringValue);
                        break;
                    case Vector2 vector2Value:
                        EditorGUILayout.Vector2Field(label, vector2Value);
                        break;
                    case Vector3 vector3Value:
                        EditorGUILayout.Vector3Field(label, vector3Value);
                        break;
                    case Color colorValue:
                        EditorGUILayout.ColorField(label, colorValue);
                        break;
                    case Enum enumValue:
                        EditorGUILayout.TextField(label, enumValue.ToString());
                        break;
                    case Object objectValue:
                        EditorGUILayout.ObjectField(label, objectValue, objectValue.GetType(), true);
                        break;
                    default:
                        EditorGUILayout.LabelField(label, value.ToString());
                        break;
                }
            }
        }
    }
}