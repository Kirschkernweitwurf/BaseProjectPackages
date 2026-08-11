using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws buttons for methods marked with <see cref="HeaderButtonAttribute"/> into the component
    /// header. Buttons sit on the bottom row of the header and are laid out right to left, clear of the
    /// help and settings icons Unity places at the top right. Methods are collected once per type.
    /// </summary>
    public static class HeaderButtonRenderer
    {
        private const float BottomMargin = 4f;
        private const float ButtonHeight = 16f;
        private const string CancelLabel = "Cancel";
        private const string ConfirmLabel = "Confirm";
        private const float Gap = 2f;

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private const float RightMargin = 6f;

        private static readonly Dictionary<Type, HeaderButton[]> Buttons = new();

        /// <summary>Draws all header buttons of the edited object into the given header rect.</summary>
        /// <param name="editor">The editor whose targets receive the invocation.</param>
        /// <param name="header">The rect the default header was drawn into.</param>
        public static void Draw(UnityEditor.Editor editor, Rect header)
        {
            if (editor.target == null)
                return;

            HeaderButton[] buttons = GetButtons(editor.target.GetType());
            if (buttons.Length == 0)
                return;

            float x = header.xMax - RightMargin;
            float y = header.yMax - ButtonHeight - BottomMargin;

            foreach (HeaderButton button in buttons)
            {
                float width = Mathf.Max(button.Attribute.Width, HeaderButtonAttribute.DefaultWidth);
                x -= width;

                Rect rect = new(x, y, width, ButtonHeight);

                using (new EditorGUI.DisabledScope(!IsEnabled(button.Attribute.Mode)))
                {
                    if (GUI.Button(rect, button.Label, EditorStyles.miniButton) && Confirm(button))
                        Invoke(editor, button);
                }

                x -= Gap;
            }
        }

        private static void Invoke(UnityEditor.Editor editor, in HeaderButton button)
        {
            foreach (Object item in editor.targets)
                button.Method.Invoke(item, null);
        }

        private static HeaderButton[] GetButtons(Type type)
        {
            if (Buttons.TryGetValue(type, out HeaderButton[] cached))
                return cached;

            List<HeaderButton> buttons = new();
            foreach (MethodInfo method in type.GetMethods(MethodFlags))
            {
                HeaderButtonAttribute attribute = method.GetCustomAttribute<HeaderButtonAttribute>();
                if (attribute == null || method.GetParameters().Length > 0)
                    continue;

                string label = string.IsNullOrEmpty(attribute.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : attribute.Label;

                buttons.Add(new HeaderButton(method, attribute, label));
            }

            HeaderButton[] result = buttons.ToArray();
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

        private static bool Confirm(in HeaderButton button)
        {
            if (string.IsNullOrEmpty(button.Attribute.Confirm))
                return true;

            return EditorUtility.DisplayDialog(button.Label, button.Attribute.Confirm, ConfirmLabel, CancelLabel);
        }
    }
}