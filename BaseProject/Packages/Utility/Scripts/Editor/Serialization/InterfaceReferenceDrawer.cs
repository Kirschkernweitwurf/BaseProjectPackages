using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws an <see cref="InterfaceReference{TInterface,TObject}"/> as a normal object field that only
    /// accepts objects implementing the interface. Dropping a GameObject or a component that does not
    /// implement it directly resolves the first component on that object which does, so the common case
    /// of dragging a whole GameObject works without hunting for the right component.
    /// </summary>
    [CustomPropertyDrawer(typeof(InterfaceReference<,>), true)]
    public sealed class InterfaceReferenceDrawer : PropertyDrawer
    {
        private const float WarningHeightLines = 2f;
        private const float WarningSpacing = 2f;

        // One drawer instance serves every element of a list, so the rejection message is tied to the
        // element it came from. Without that the height of one row would follow another row's warning.
        private string _warning;
        private string _warningPath;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;

            return HasWarningFor(property)
                ? line + WarningSpacing + line * WarningHeightLines
                : line;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty underlying =
                property.FindPropertyRelative(InterfaceReference<object, Object>.UnderlyingField);

            if (underlying == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            if (!TryResolveArguments(fieldInfo, out Type interfaceType, out Type objectType))
            {
                EditorGUI.PropertyField(position, underlying, label);
                return;
            }

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, underlying);
            DrawField(fieldRect, property, underlying, label, interfaceType, objectType);
            EditorGUI.EndProperty();

            if (!HasWarningFor(property))
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                EditorGUIUtility.singleLineHeight * WarningHeightLines);

            EditorGUI.HelpBox(warningRect, _warning, MessageType.Warning);
        }

        // The assigned object wins when it already implements the interface. Otherwise the first
        // component on the same GameObject that does is used, which is what dragging a prefab means.
        private static Object Resolve(Object assigned, Type interfaceType)
        {
            if (interfaceType.IsInstanceOfType(assigned))
                return assigned;

            GameObject gameObject = assigned switch
            {
                GameObject direct => direct,
                Component component => component.gameObject,
                _ => null
            };

            if (gameObject == null)
                return null;

            foreach (Component candidate in gameObject.GetComponents<Component>())
            {
                if (candidate != null && interfaceType.IsInstanceOfType(candidate))
                    return candidate;
            }

            return null;
        }

        private static bool TryResolveArguments(FieldInfo field, out Type interfaceType, out Type objectType)
        {
            interfaceType = null;
            objectType = null;

            if (field == null)
                return false;

            Type current = ElementType(field.FieldType);

            while (current != null)
            {
                if (current.IsGenericType
                    && current.GetGenericTypeDefinition() == typeof(InterfaceReference<,>))
                {
                    Type[] arguments = current.GetGenericArguments();
                    interfaceType = arguments[0];
                    objectType = arguments[1];
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        // Unwraps arrays and lists so the drawer works on collections of interface references too.
        private static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private void DrawField(Rect rect, SerializedProperty property, SerializedProperty underlying,
            GUIContent label, Type interfaceType, Type objectType)
        {
            GUIContent content = new($"{label.text} ({interfaceType.Name})", label.tooltip);
            bool allowSceneObjects = !EditorUtility.IsPersistent(underlying.serializedObject.targetObject);

            Object previous = underlying.objectReferenceValue;
            Object assigned = EditorGUI.ObjectField(rect, content, previous, objectType, allowSceneObjects);

            if (assigned == previous)
                return;

            if (assigned == null)
            {
                underlying.objectReferenceValue = null;
                ClearWarning();
                return;
            }

            Object resolved = Resolve(assigned, interfaceType);
            if (resolved == null)
            {
                _warning = $"{assigned.name} does not implement {interfaceType.Name}.";
                _warningPath = property.propertyPath;
                return;
            }

            underlying.objectReferenceValue = resolved;
            ClearWarning();
        }

        private bool HasWarningFor(SerializedProperty property)
            => !string.IsNullOrEmpty(_warning) && _warningPath == property.propertyPath;

        private void ClearWarning()
        {
            _warning = null;
            _warningPath = null;
        }
    }
}