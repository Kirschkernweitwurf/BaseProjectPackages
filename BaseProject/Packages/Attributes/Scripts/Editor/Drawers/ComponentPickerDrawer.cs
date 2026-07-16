using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws fields marked with <see cref="ComponentPickerAttribute"/> and resolves drops of GameObjects
    /// that carry more than one component of the field type.
    /// </summary>
    [CustomPropertyDrawer(typeof(ComponentPickerAttribute))]
    public class ComponentPickerDrawer : PropertyDrawer
    {
        private const string ArrayToken = ".Array.data[";
        private const float BadgeWidth = 32f;
        private const float Spacing = 2f;
        private const string UsageError = "[ComponentPicker] only works on component reference fields.";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type componentType = GetComponentType();
            if (property.propertyType != SerializedPropertyType.ObjectReference
                || componentType == null
                || !typeof(Component).IsAssignableFrom(componentType))
            {
                EditorGUI.LabelField(position, label, new GUIContent(UsageError));
                return;
            }

            HandleDragAndDrop(position, property, componentType);

            ComponentPickerAttribute pickerAttribute = (ComponentPickerAttribute)attribute;
            Component[] siblings = GetSiblings(property, componentType);
            bool isBadgeVisible = pickerAttribute.ShowIndexBadge && siblings.Length > 1;

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

        private static List<Component> CollectCandidates(Type componentType)
        {
            List<Component> result = new();
            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                GameObject gameObject = draggedObject as GameObject;
                if (gameObject == null)
                    continue;

                result.AddRange(gameObject.GetComponents(componentType));
            }

            return result;
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
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
                return;

            property.objectReferenceValue = component;
            serializedObject.ApplyModifiedProperties();
        }

        private static void AddAll(SerializedObject serializedObject, string arrayPath, List<Component> components)
        {
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

        private void HandleDragAndDrop(Rect position, SerializedProperty property, Type componentType)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
                return;

            if (!position.Contains(currentEvent.mousePosition))
                return;

            List<Component> candidates = CollectCandidates(componentType);
            if (candidates.Count < 2)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            if (currentEvent.type != EventType.DragPerform)
            {
                currentEvent.Use();
                return;
            }

            DragAndDrop.AcceptDrag();
            currentEvent.Use();
            ShowPickerMenu(property, candidates);
        }

        private void ShowPickerMenu(SerializedProperty property, List<Component> candidates)
        {
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            ComponentPickerAttribute pickerAttribute = (ComponentPickerAttribute)attribute;

            GenericMenu menu = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                Component candidate = candidates[i];
                GUIContent entry = new($"#{i}  {candidate.GetType().Name}  ({candidate.gameObject.name})");
                menu.AddItem(entry, candidate == property.objectReferenceValue,
                    func: () => AssignSingle(serializedObject, propertyPath, candidate));
            }

            SerializedProperty arrayProperty = GetParentArray(property);
            if (pickerAttribute.AllowFillList && arrayProperty != null)
            {
                string arrayPath = arrayProperty.propertyPath;
                List<Component> all = new(candidates);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent($"Add All ({all.Count})"), false,
                    func: () => AddAll(serializedObject, arrayPath, all));
            }

            menu.ShowAsContext();
        }

        private void DrawIndexBadge(Rect position, SerializedProperty property, Component[] siblings)
        {
            Component component = property.objectReferenceValue as Component;
            int index = component == null
                ? -1
                : Array.IndexOf(siblings, component);

            string text = index < 0
                ? "#?"
                : $"#{index}";

            GUIContent content = new(text, "Click to switch to another component of the same type.");

            if (!GUI.Button(position, content, EditorStyles.miniButton))
                return;

            ShowPickerMenu(property, new List<Component>(siblings));
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