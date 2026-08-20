using System;
using System.Collections.Generic;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a dropdown of Animator parameters for <see cref="AnimatorParamAttribute"/>.
    /// Stores the name on a string field and the hash on an int field. While the Animator reference is
    /// missing, has no controller or offers no matching parameters, the plain field stays editable and
    /// a compact warning below explains what is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    internal sealed class AnimatorParamDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<AnimatorParamAttribute>("a string or int");

        private string[] _names;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
            => Evaluate(property, (AnimatorParamAttribute)attribute, out _names);

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (complete)
                DrawDropdown(rect, property, label, _names);
            else
                EditorGUI.PropertyField(rect, property, label);
        }

        private static void DrawDropdown(Rect rect, SerializedProperty property, GUIContent label, string[] names)
        {
            bool isString = property.propertyType == SerializedPropertyType.String;

            int current = CurrentIndex(property, names, isString);
            int selected = LabeledField.Popup(rect, label, current, names);
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

            if (!MemberValueResolver.TryResolveSibling(property, attribute.AnimatorField, out Animator animator))
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
    }
}