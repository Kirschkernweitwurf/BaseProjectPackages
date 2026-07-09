using System;
using System.Reflection;
using Base.AttributePackage.Layout;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Draws read-only values for members marked with <see cref="ShowNonSerializedAttribute"/>
    /// and <see cref="ShowNativePropertyAttribute"/>.
    /// </summary>
    public static class NativeMemberRenderer
    {
        /// <summary>Draws all native members for the edited object.</summary>
        public static void Draw(UnityEditor.Editor editor)
        {
            Object target = editor.target;
            Type type = target.GetType();

            foreach (FieldInfo field in ReflectionCache.AllFields(type))
            {
                if (field.GetCustomAttribute<ShowNonSerializedAttribute>() != null)
                    DrawValue(ObjectNames.NicifyVariableName(field.Name), field.GetValue(target));
            }

            foreach (PropertyInfo property in ReflectionCache.AllProperties(type))
            {
                if (!property.CanRead || property.GetCustomAttribute<ShowNativePropertyAttribute>() == null)
                    continue;

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