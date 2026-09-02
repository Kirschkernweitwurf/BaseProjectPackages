using System;
using System.Collections.Generic;
using Base.AttributesPackage.Editor.Core;
using Base.UtilityPackage.Editor;
using Base.UtilityPackage.Editor.Dropdown;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Drawers
{
    /// <summary>
    /// Draws a type picker above a <c>[SerializeReference]</c> field for
    /// <see cref="ReferencePickerAttribute"/>, then the fields of the chosen instance. Without this the
    /// inspector offers no way to create or swap the concrete type of managed reference.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReferencePickerAttribute))]
    internal sealed class ReferencePickerDrawer : PropertyDrawer
    {
        private const string ClearLabel = "Clear";
        private const string UnsupportedMessage = "Use [ReferencePicker] with a [SerializeReference] field.";

        // Kept alive between openings so the dropdown remembers its scroll position and search text.
        private readonly AdvancedDropdownState _state = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return line;

            if (!property.isExpanded || property.managedReferenceValue == null)
                return line;

            return line + ChildrenHeight(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                LabeledField.Hint(position, label, UnsupportedMessage);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            Rect header = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawHeader(header, property, label);

            if (property.isExpanded && property.managedReferenceValue != null)
                DrawChildren(position, property, header.yMax);

            EditorGUI.EndProperty();
        }

        private static float ChildrenHeight(SerializedProperty property)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = 0f;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                height += spacing + EditorGUI.GetPropertyHeight(iterator, true);
            }

            return height;
        }

        private static void DrawChildren(Rect position, SerializedProperty property, float top)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float y = top;

            EditorGUI.indentLevel++;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = property.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                float height = EditorGUI.GetPropertyHeight(iterator, true);
                y += spacing;

                EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), iterator.Copy(), true);
                y += height;
            }

            EditorGUI.indentLevel--;
        }

        private static string CurrentLabel(SerializedProperty property)
        {
            object value = property.managedReferenceValue;

            return value == null
                ? ReferencePickerAttribute.NullLabel
                : value.GetType().Name;
        }

        private void DrawHeader(Rect rect, SerializedProperty property, GUIContent label)
        {
            Rect foldoutRect = new(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            Rect buttonRect = new(rect.x + EditorGUIUtility.labelWidth, rect.y,
                rect.width - EditorGUIUtility.labelWidth, rect.height);

            if (!EditorGUI.DropdownButton(buttonRect, new GUIContent(CurrentLabel(property)), FocusType.Keyboard))
                return;

            if (!ManagedReferenceTypes.TryResolveFieldType(property, out Type declaredType))
                return;

            ShowPicker(buttonRect, property, declaredType);
        }

        private void ShowPicker(Rect rect, SerializedProperty property, Type declaredType)
        {
            Type[] candidates = ManagedReferenceTypes.GetAssignable(declaredType);

            List<string> labels = new(candidates.Length + 1)
            {
                ClearLabel
            };

            foreach (Type candidate in candidates)
                labels.Add(ManagedReferenceTypes.LabelFor(candidate));

            // The property is captured for the callback, which runs after this OnGUI call has returned.
            SerializedProperty captured = property.Copy();

            SearchableDropdown menu = new(_state, declaredType.Name, labels, onSelected: index =>
            {
                captured.managedReferenceValue = index <= 0
                    ? null
                    : Activator.CreateInstance(candidates[index - 1]);

                captured.serializedObject.ApplyModifiedProperties();
            });

            menu.Show(rect);
        }
    }
}