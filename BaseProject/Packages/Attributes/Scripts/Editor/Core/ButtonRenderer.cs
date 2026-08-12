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
    /// <remarks>
    /// A method taking parameters gets a field for each above its button. Those values are editor state
    /// held by <see cref="ButtonArguments"/> rather than serialized on the object, so a one-off call
    /// costs nothing the game will ever carry.
    /// <para>
    /// Buttons group the same way fields do: a run of consecutive buttons sharing a row name is drawn
    /// side by side, and a run sharing a foldout name folds away together. A button in a row cannot also
    /// carry parameters, since the argument fields need the width the row is dividing up.
    /// </para>
    /// </remarks>
    internal static class ButtonRenderer
    {
        private const string CancelLabel = "Cancel";
        private const string ConfirmLabel = "Confirm";
        private const string FoldoutKeyPrefix = "BUTTONS";
        private const float LargeHeightScale = 1.5f;

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, InspectorButton[]> Buttons = new();

        /// <summary>Draws all buttons for the edited object.</summary>
        /// <param name="editor">The editor whose target owns the buttons.</param>
        public static void Draw(UnityEditor.Editor editor)
        {
            Type type = editor.target.GetType();
            InspectorButton[] buttons = GetButtons(type);

            int index = 0;

            while (index < buttons.Length)
                index = DrawBlock(editor, type, buttons, index);
        }

        // A block is a run of consecutive buttons sharing a foldout, or a single button without one.
        private static int DrawBlock(UnityEditor.Editor editor, Type type, InspectorButton[] buttons, int start)
        {
            string foldout = buttons[start].Attribute.Foldout;

            if (string.IsNullOrEmpty(foldout))
                return DrawRow(editor, buttons, start);

            int end = start;
            while (end < buttons.Length && buttons[end].Attribute.Foldout == foldout)
                end++;

            string key = StateKey.For(type, FoldoutKeyPrefix, foldout);
            bool stored = EditorPrefs.GetBool(key, buttons[start].Attribute.DefaultExpanded);
            bool expanded = EditorGUILayout.Foldout(stored, foldout, true);

            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            if (!expanded)
                return end;

            EditorGUI.indentLevel++;

            int index = start;
            while (index < end)
                index = DrawRow(editor, buttons, index);

            EditorGUI.indentLevel--;

            return end;
        }

        // A row is a run of consecutive buttons sharing a row name, or a single button without one.
        private static int DrawRow(UnityEditor.Editor editor, InspectorButton[] buttons, int start)
        {
            string row = buttons[start].Attribute.Row;

            if (string.IsNullOrEmpty(row))
            {
                DrawOne(editor, buttons[start]);
                return start + 1;
            }

            int end = start;
            while (end < buttons.Length && buttons[end].Attribute.Row == row)
                end++;

            EditorGUILayout.BeginHorizontal();

            for (int i = start; i < end; i++)
                DrawOne(editor, buttons[i]);

            EditorGUILayout.EndHorizontal();

            return end;
        }

        private static void DrawOne(UnityEditor.Editor editor, in InspectorButton button)
        {
            using (new EditorGUI.DisabledScope(!IsEnabled(button.Attribute.Mode)))
            {
                // The argument fields are drawn inside the same disabled scope as the button, so a
                // play-mode-only call cannot be set up while it could not be made anyway.
                object[] arguments = ButtonArguments.Draw(editor.target, button.Method);

                if (GUILayout.Button(button.Label, GUILayout.Height(HeightOf(button))) && Confirm(button))
                    Invoke(editor, button, arguments);
            }
        }

        private static float HeightOf(in InspectorButton button)
            => button.Attribute.Size == EButtonSize.Large
                ? EditorGUIUtility.singleLineHeight * LargeHeightScale
                : EditorGUIUtility.singleLineHeight;

        // The arguments are shared across a multi-object selection, because they belong to the call
        // rather than to any one of the objects it is made on.
        private static void Invoke(UnityEditor.Editor editor, in InspectorButton button, object[] arguments)
        {
            Type declaring = button.Method.DeclaringType;

            foreach (Object item in editor.targets)
            {
                if (item != null && declaring != null && declaring.IsInstanceOfType(item))
                    button.Method.Invoke(item, arguments);
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

                // A parameter this package cannot draw would leave a button that throws when pressed, so
                // the method is skipped and the troubleshoot window reports why.
                if (attribute == null || !ButtonArguments.IsSupported(method))
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

            return EditorUtility.DisplayDialog(button.Label, button.Attribute.Confirm, ConfirmLabel,
                CancelLabel);
        }
    }
}