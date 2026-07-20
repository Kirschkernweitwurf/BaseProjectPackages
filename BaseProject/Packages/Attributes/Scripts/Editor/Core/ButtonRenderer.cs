using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws inspector buttons for methods marked with <see cref="ButtonAttribute"/>. The annotated
    /// methods and their labels are collected once per type and cached, so repaints do not run any
    /// reflection.
    /// </summary>
    public static class ButtonRenderer
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, InspectorButton[]> Buttons = new();

        /// <summary>Draws all buttons for the edited object.</summary>
        public static void Draw(UnityEditor.Editor editor)
        {
            foreach (InspectorButton button in GetButtons(editor.target.GetType()))
            {
                using (new EditorGUI.DisabledScope(!IsEnabled(button.Attribute.Mode)))
                {
                    if (GUILayout.Button(button.Label) && Confirm(button))
                        foreach (Object item in editor.targets)
                            button.Method.Invoke(item, null);
                }
            }
        }

        private static InspectorButton[] GetButtons(Type type)
        {
            if (Buttons.TryGetValue(type, out InspectorButton[] cached))
                return cached;

            List<InspectorButton> buttons = new();
            foreach (MethodInfo method in type.GetMethods(MethodFlags))
            {
                ButtonAttribute attribute = method.GetCustomAttribute<ButtonAttribute>();
                if (attribute == null || method.GetParameters().Length > 0)
                    continue;

                string label = string.IsNullOrEmpty(attribute.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attribute.Label;

                buttons.Add(new InspectorButton(method, attribute, label));
            }

            InspectorButton[] result = buttons.ToArray();
            Buttons[type] = result;
            return result;
        }

        private static bool IsEnabled(EButtonMode mode)
        {
            switch (mode)
            {
                case EButtonMode.PlayMode:
                    return Application.isPlaying;
                case EButtonMode.EditMode:
                    return !Application.isPlaying;
                default:
                    return true;
            }
        }

        private static bool Confirm(in InspectorButton button)
        {
            if (string.IsNullOrEmpty(button.Attribute.Confirm))
                return true;

            return EditorUtility.DisplayDialog(button.Label, button.Attribute.Confirm, "Confirm", "Cancel");
        }
    }
}