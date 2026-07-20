using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of member-provided values for <see cref="DropdownAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(DropdownAttribute))]
    public sealed class DropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DropdownAttribute dropdown = (DropdownAttribute)attribute;
            List<object> values = ResolveOptions(property, dropdown.Member);

            if (values == null || values.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] labels = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
                labels[i] = values[i]?.ToString() ?? "null";

            int current = CurrentIndex(property, values);

            EditorGUI.BeginProperty(position, label, property);
            int selected = EditorGUI.Popup(position, label.text, current, labels);
            if (selected >= 0 && selected < values.Count)
                SetValue(property, values[selected]);

            EditorGUI.EndProperty();
        }

        private static List<object> ResolveOptions(SerializedProperty property, string member)
        {
            Object target = property.serializedObject.targetObject;
            Type type = target.GetType();

            if (!MemberValueResolver.TryResolve(type, target, member, out object raw))
            {
                MethodInfo method = ReflectionCache.GetMethod(type, member);
                if (method != null && method.GetParameters().Length == 0)
                    raw = method.Invoke(target, null);
            }

            if (raw is string || !(raw is IEnumerable enumerable))
                return null;

            List<object> list = new();
            foreach (object item in enumerable)
                list.Add(item);

            return list;
        }

        private static int CurrentIndex(SerializedProperty property, List<object> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (Matches(property, values[i]))
                    return i;
            }

            return -1;
        }

        private static bool Matches(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    return property.stringValue == value as string;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                    return value != null && property.intValue == Convert.ToInt32(value);
                case SerializedPropertyType.Float:
                    return value != null && Mathf.Approximately(property.floatValue, Convert.ToSingle(value));
                case SerializedPropertyType.Boolean:
                    return value is bool boolValue && property.boolValue == boolValue;
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == value as Object;
                default:
                    return false;
            }
        }

        private static void SetValue(SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.String:
                    property.stringValue = value as string ?? string.Empty;
                    break;
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Enum:
                    property.intValue = Convert.ToInt32(value);
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value is bool boolValue && boolValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    break;
            }
        }
    }
}