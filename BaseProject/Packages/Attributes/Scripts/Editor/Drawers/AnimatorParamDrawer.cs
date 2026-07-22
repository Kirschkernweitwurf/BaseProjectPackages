using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of Animator parameters for <see cref="AnimatorParamAttribute"/>.
    /// Stores the name on a string field and the hash on an int field. While the Animator reference is
    /// missing, has no controller or offers no matching parameters, the plain field stays editable and
    /// a compact warning below explains what is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public sealed class AnimatorParamDrawer : PropertyDrawer
    {
        private const float WarningSpacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
                return EditorGUIUtility.singleLineHeight;

            string warning = Evaluate(property, (AnimatorParamAttribute)attribute, out _);
            return warning == null
                ? EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight + WarningSpacing + CompactHelpBox.Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsSupported(property))
            {
                EditorGUI.LabelField(position, label.text,
                    AttributeNames.Usage<AnimatorParamAttribute>("a string or int"));

                return;
            }

            string warning = Evaluate(property, (AnimatorParamAttribute)attribute, out string[] names);

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, property);

            if (warning == null)
                DrawDropdown(fieldRect, property, label, names);
            else
                EditorGUI.PropertyField(fieldRect, property, label);

            EditorGUI.EndProperty();

            if (warning == null)
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                CompactHelpBox.Height);

            CompactHelpBox.Draw(warningRect, warning, EInfoBoxType.Warning);
        }

        private static void DrawDropdown(Rect rect, SerializedProperty property, GUIContent label,
            string[] names)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;

            int current = CurrentIndex(property, names, isString);
            int selected = EditorGUI.Popup(rect, label.text, current, names);
            if (selected < 0 || selected >= names.Length || selected == current)
                return;

            if (isString)
                property.stringValue = names[selected];
            else
                property.intValue = Animator.StringToHash(names[selected]);
        }

        private static string Evaluate(SerializedProperty property, AnimatorParamAttribute attribute,
            out string[] names)
        {
            names = null;

            if (!TryResolveAnimator(property, attribute.AnimatorField, out Animator animator))
                return $"Animator field '{attribute.AnimatorField}' was not found on this object.";

            if (animator == null)
                return $"Animator field '{attribute.AnimatorField}' is not assigned.";

            AnimatorController controller = ResolveController(animator);
            if (controller == null)
                return "The assigned Animator has no AnimatorController.";

            names = CollectNames(controller, attribute);
            if (names.Length > 0)
                return null;

            return attribute.HasFilter
                ? $"The AnimatorController has no {attribute.Type} parameters."
                : "The AnimatorController has no parameters.";
        }

        private static bool TryResolveAnimator(SerializedProperty property, string animatorField,
            out Animator animator)
        {
            animator = null;
            Object target = property.serializedObject.targetObject;
            FieldInfo field = ReflectionCache.GetField(target.GetType(), animatorField);
            if (field == null || field.FieldType != typeof(Animator))
                return false;

            animator = field.GetValue(target) as Animator;
            return true;
        }

        private static AnimatorController ResolveController(Animator animator)
        {
            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController)
                runtimeController = overrideController.runtimeAnimatorController;

            return runtimeController as AnimatorController;
        }

        private static string[] CollectNames(AnimatorController controller, AnimatorParamAttribute attribute)
        {
            List<string> names = new();
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (!attribute.HasFilter || parameter.type == attribute.Type)
                    names.Add(parameter.name);
            }

            return names.ToArray();
        }

        private static int CurrentIndex(SerializedProperty property, string[] names, bool isString)
        {
            if (isString)
                return Array.IndexOf(names, property.stringValue);

            int hash = property.intValue;
            for (int i = 0; i < names.Length; i++)
            {
                if (Animator.StringToHash(names[i]) == hash)
                    return i;
            }

            return -1;
        }

        private static bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;
    }
}