using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a dropdown of Animator parameters for <see cref="AnimatorParamAttribute"/>.
    /// Stores the name on a string field and the hash on an int field.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimatorParamAttribute))]
    public sealed class AnimatorParamDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AnimatorParamAttribute attribute = (AnimatorParamAttribute)this.attribute;

            bool isString = property.propertyType == SerializedPropertyType.String;
            bool isInt = property.propertyType == SerializedPropertyType.Integer;
            if (!isString && !isInt)
            {
                EditorGUI.LabelField(position, label.text, "Use [AnimatorParam] with a string or int.");
                return;
            }

            AnimatorController controller = ResolveController(property, attribute.AnimatorField);
            if (controller == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            List<string> names = new();
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (!attribute.HasFilter || parameter.type == attribute.Type)
                    names.Add(parameter.name);
            }

            if (names.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            int current = CurrentIndex(property, names, isString);
            int selected = EditorGUI.Popup(position, label.text, current, names.ToArray());
            if (selected >= 0 && selected < names.Count)
            {
                if (isString)
                    property.stringValue = names[selected];
                else
                    property.intValue = Animator.StringToHash(names[selected]);
            }

            EditorGUI.EndProperty();
        }

        private static AnimatorController ResolveController(SerializedProperty property, string animatorField)
        {
            Object target = property.serializedObject.targetObject;
            FieldInfo field = ReflectionCache.GetField(target.GetType(), animatorField);

            if (!(field?.GetValue(target) is Animator animator) || animator == null)
                return null;

            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController)
                runtimeController = overrideController.runtimeAnimatorController;

            return runtimeController as AnimatorController;
        }

        private static int CurrentIndex(SerializedProperty property, List<string> names, bool isString)
        {
            if (isString)
                return names.IndexOf(property.stringValue);

            int hash = property.intValue;
            for (int i = 0; i < names.Count; i++)
            {
                if (Animator.StringToHash(names[i]) == hash)
                    return i;
            }

            return -1;
        }
    }
}