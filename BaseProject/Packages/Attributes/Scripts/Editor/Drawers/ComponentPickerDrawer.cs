using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws fields marked with <see cref="ComponentPickerAttribute"/>. Dropping a GameObject assigns
    /// the first matching component through Unity's default handling. Every assigned field shows an
    /// index badge that opens a picker menu for switching between the sibling components of the field
    /// type; the badge is disabled while the GameObject holds no other component of that type. On
    /// list elements the menu also offers adding every matching component at once.
    /// </summary>
    [CustomPropertyDrawer(typeof(ComponentPickerAttribute))]
    public sealed class ComponentPickerDrawer : PropertyDrawer
    {
        private const string ArrayToken = ".Array.data[";
        private const float BadgeWidth = 32f;
        private const float Spacing = 2f;

        private static GUIContent _usageErrorContent;

        private static readonly string UsageError =
            $"[{AttributeNames.Display<ComponentPickerAttribute>()}] only works on component reference fields.";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type componentType = GetComponentType();
            if (property.propertyType != SerializedPropertyType.ObjectReference
                || componentType == null
                || !typeof(Component).IsAssignableFrom(componentType))
            {
                EditorGUI.LabelField(position, label, _usageErrorContent ??= new GUIContent(UsageError));
                return;
            }

            Component[] siblings = GetSiblings(property, componentType);
            bool isBadgeVisible = property.objectReferenceValue != null;

            Rect fieldRect = position;
            if (isBadgeVisible)
                fieldRect.width -= BadgeWidth + Spacing;

            EditorGUI.PropertyField(fieldRect, property, label);

            if (!isBadgeVisible)
                return;

            Rect badgeRect = new(position.xMax - BadgeWidth, position.y, BadgeWidth, EditorGUIUtility.singleLineHeight);
            DrawIndexBadge(badgeRect, property, siblings);
        }

        private static Component[] GetSiblings(SerializedProperty property, Type componentType)
        {
            Component component = property.objectReferenceValue as Component;
            if (component == null)
                return Array.Empty<Component>();

            return component.GetComponents(componentType);
        }

        private static SerializedProperty GetParentArray(SerializedProperty property)
        {
            int tokenIndex = property.propertyPath.LastIndexOf(ArrayToken, StringComparison.Ordinal);
            if (tokenIndex < 0)
                return null;

            string parentPath = property.propertyPath.Substring(0, tokenIndex);
            return property.serializedObject.FindProperty(parentPath);
        }

        private static void AssignSingle(SerializedObject serializedObject, string propertyPath, Component component)
        {
            if (serializedObject.targetObject == null)
                return;

            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                return;

            property.objectReferenceValue = component;
            serializedObject.ApplyModifiedProperties();
        }

        private static void AddAll(SerializedObject serializedObject, string arrayPath, List<Component> components)
        {
            if (serializedObject.targetObject == null)
                return;

            serializedObject.Update();
            SerializedProperty arrayProperty = serializedObject.FindProperty(arrayPath);
            if (arrayProperty == null || !arrayProperty.isArray)
                return;

            foreach (Component component in components)
            {
                if (IsInArray(arrayProperty, component))
                    continue;

                int freeIndex = GetFreeIndex(arrayProperty);
                if (freeIndex < 0)
                {
                    arrayProperty.arraySize++;
                    freeIndex = arrayProperty.arraySize - 1;
                }

                arrayProperty.GetArrayElementAtIndex(freeIndex).objectReferenceValue = component;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static bool IsInArray(SerializedProperty arrayProperty, Component component)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == component)
                    return true;
            }

            return false;
        }

        private static int GetFreeIndex(SerializedProperty arrayProperty)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                if (arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    return i;
            }

            return -1;
        }

        private static void ShowPickerMenu(SerializedObject serializedObject, string propertyPath,
            List<Component> candidates)
        {
            if (serializedObject.targetObject == null)
                return;

            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                return;

            GenericMenu menu = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                Component candidate = candidates[i];
                GUIContent entry = new($"#{i}  {candidate.GetType().Name}  ({candidate.gameObject.name})");
                menu.AddItem(entry, candidate == property.objectReferenceValue,
                    func: () => AssignSingle(serializedObject, propertyPath, candidate));
            }

            SerializedProperty arrayProperty = GetParentArray(property);
            if (arrayProperty != null)
            {
                string arrayPath = arrayProperty.propertyPath;
                List<Component> all = new(candidates);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent($"Add All ({all.Count})"), false,
                    func: () => AddAll(serializedObject, arrayPath, all));
            }

            menu.ShowAsContext();
        }

        private static void DrawIndexBadge(Rect position, SerializedProperty property, Component[] siblings)
        {
            Component component = property.objectReferenceValue as Component;
            int index = component == null
                ? -1
                : Array.IndexOf(siblings, component);

            string text = index < 0
                ? "#?"
                : $"#{index}";

            bool hasAlternatives = siblings.Length > 1;
            GUIContent content = new(text, hasAlternatives
                ? "Click to switch to another component of the same type."
                : "The GameObject holds no other component of this type.");

            using (new EditorGUI.DisabledScope(!hasAlternatives))
            {
                if (GUI.Button(position, content, EditorStyles.miniButton))
                    ShowPickerMenu(property.serializedObject, property.propertyPath, new List<Component>(siblings));
            }
        }

        private Type GetComponentType()
        {
            Type type = fieldInfo.FieldType;
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}