using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws inspector buttons for methods marked with <see cref="ButtonAttribute"/>.
    /// </summary>
    public static class ButtonRenderer
    {
        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>Draws all buttons for the edited object.</summary>
        public static void Draw(UnityEditor.Editor editor)
        {
            MethodInfo[] methods = editor.target.GetType().GetMethods(MethodFlags);
            foreach (MethodInfo method in methods)
            {
                ButtonAttribute button = method.GetCustomAttribute<ButtonAttribute>();
                if (button == null || method.GetParameters().Length > 0)
                    continue;

                string label = string.IsNullOrEmpty(button.Label)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : button.Label;

                using (new EditorGUI.DisabledScope(!IsEnabled(button.Mode)))
                {
                    if (GUILayout.Button(label) && Confirm(button, label))
                        foreach (Object item in editor.targets)
                            method.Invoke(item, null);
                }
            }
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

        private static bool Confirm(ButtonAttribute button, string label)
        {
            if (string.IsNullOrEmpty(button.Confirm))
                return true;

            return EditorUtility.DisplayDialog(label, button.Confirm, "Confirm", "Cancel");
        }
    }
}