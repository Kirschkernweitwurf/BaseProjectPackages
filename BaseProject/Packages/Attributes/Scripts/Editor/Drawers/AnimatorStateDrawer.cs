using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of the states of a sibling Animator's controller for
    /// <see cref="AnimatorStateAttribute"/>. Stores the name on a string field and the hash on an int
    /// field. While the Animator reference is missing, has no controller or offers no states, the plain
    /// field stays editable and a compact warning below explains what is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
    internal sealed class AnimatorStateDrawer : WarningFieldDrawer
    {
        protected override string UsageMessage => AttributeNames.Usage<AnimatorStateAttribute>("a string or int");

        private string[] _paths;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String
                || property.propertyType == SerializedPropertyType.Integer;

        protected override string Evaluate(SerializedProperty property)
            => Evaluate(property, (AnimatorStateAttribute)attribute, out _paths);

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            bool isString = property.propertyType == SerializedPropertyType.String;
            int current = CurrentIndex(property, _paths, isString);
            int selected = LabeledField.Popup(rect, label, current, _paths);

            if (selected < 0 || selected >= _paths.Length || selected == current)
                return;

            if (isString)
                property.stringValue = _paths[selected];
            else
                property.intValue = Animator.StringToHash(_paths[selected]);
        }

        private static string Evaluate(SerializedProperty property, AnimatorStateAttribute attribute,
            out string[] paths)
        {
            paths = null;

            if (!MemberValueResolver.TryResolveSibling(property, attribute.AnimatorField, out Animator animator))
                return $"Animator field '{attribute.AnimatorField}' was not found on this object.";

            if (animator == null)
                return $"Animator field '{attribute.AnimatorField}' is not assigned.";

            AnimatorController controller = ResolveController(animator);
            if (controller == null)
                return "The assigned Animator has no AnimatorController.";

            paths = CollectPaths(controller);
            return paths.Length > 0
                ? null
                : "The AnimatorController has no states.";
        }

        private static AnimatorController ResolveController(Animator animator)
        {
            RuntimeAnimatorController runtimeController = animator.runtimeAnimatorController;
            if (runtimeController is AnimatorOverrideController overrideController)
                runtimeController = overrideController.runtimeAnimatorController;

            return runtimeController as AnimatorController;
        }

        // State names are only unique per layer, so the layer name is prefixed. That also matches the
        // dotted path Animator.Play accepts, which is what the stored value is usually fed into.
        private static string[] CollectPaths(AnimatorController controller)
        {
            List<string> paths = new();

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                if (layer.stateMachine == null)
                    continue;

                Collect(layer.stateMachine, layer.name, paths);
            }

            return paths.ToArray();
        }

        private static void Collect(AnimatorStateMachine machine, string prefix, List<string> paths)
        {
            foreach (ChildAnimatorState child in machine.states)
            {
                if (child.state != null)
                    paths.Add(prefix + AnimatorStateAttribute.LayerSeparator + child.state.name);
            }

            foreach (ChildAnimatorStateMachine child in machine.stateMachines)
            {
                if (child.stateMachine != null)
                    Collect(child.stateMachine, prefix
                        + AnimatorStateAttribute.LayerSeparator
                        + child.stateMachine.name, paths);
            }
        }

        private static int CurrentIndex(SerializedProperty property, string[] paths, bool isString)
        {
            if (isString)
                return Array.IndexOf(paths, property.stringValue);

            int hash = property.intValue;
            for (int i = 0; i < paths.Length; i++)
            {
                if (Animator.StringToHash(paths[i]) == hash)
                    return i;
            }

            return -1;
        }
    }
}