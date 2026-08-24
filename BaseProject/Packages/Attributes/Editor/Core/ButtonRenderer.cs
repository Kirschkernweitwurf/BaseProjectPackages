using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Drawers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Draws inspector buttons for methods marked with <see cref="ButtonAttribute"/>. The annotated
    /// methods and their labels are collected once per type and cached, so repaints do not run any
    /// reflection.
    /// </summary>
    /// <remarks>
    /// Buttons are grouped by name rather than by adjacency. Reflection does not report methods in
    /// declaration order, so a run of consecutive entries is not a reliable block: two buttons sharing a
    /// foldout could arrive with a third between them and the heading would be drawn twice.
    /// <para>
    /// A method taking parameters gets a field for each above its button. Those values are editor state
    /// held by <see cref="ButtonArguments"/> rather than serialized on the object, so a one-off call
    /// costs nothing the game will ever carry.
    /// </para>
    /// </remarks>
    internal static class ButtonRenderer
    {
        private const string CancelLabel = "Cancel";
        private const string ConfirmLabel = "Confirm";
        private const float LargeHeightScale = 1.5f;

        private const BindingFlags MethodFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, InspectorButton[]> Buttons = new();

        // Reused between draws so grouping does not allocate per repaint.
        private static readonly List<string> FoldoutOrder = new();

        private static readonly Dictionary<string, List<InspectorButton>> Blocks = new();

        /// <summary>Draws all buttons for the edited object.</summary>
        /// <param name="editor">The editor whose target owns the buttons.</param>
        public static void Draw(UnityEditor.Editor editor)
        {
            Type type = editor.target.GetType();

            Group(GetButtons(type));

            foreach (string foldout in FoldoutOrder)
                DrawBlock(editor, type, foldout, Blocks[foldout]);
        }

        // Buttons without a foldout keep their own one-entry block, so they stay where they were rather
        // than being gathered under an empty heading.
        private static void Group(InspectorButton[] buttons)
        {
            FoldoutOrder.Clear();
            Blocks.Clear();

            for (int i = 0; i < buttons.Length; i++)
            {
                string foldout = buttons[i].Attribute.Foldout;
                string key = string.IsNullOrEmpty(foldout)
                    ? i.ToString()
                    : foldout;

                if (!Blocks.TryGetValue(key, out List<InspectorButton> block))
                {
                    block = new List<InspectorButton>();
                    Blocks[key] = block;
                    FoldoutOrder.Add(key);
                }

                block.Add(buttons[i]);
            }
        }

        private static void DrawBlock(UnityEditor.Editor editor, Type type, string foldout,
            List<InspectorButton> block)
        {
            string heading = block[0].Attribute.Foldout;

            if (string.IsNullOrEmpty(heading))
            {
                DrawRows(editor, block);
                return;
            }

            // The heading is drawn as a section title, so a block of buttons carries the same weight as a
            // block of fields rather than reading as a stray foldout arrow. The title renderer owns the
            // expanded state, keyed on the heading, so nothing is stored twice.
            TitleAttribute title = BuildTitle(block[0].Attribute, heading);

            if (!TitleRenderer.DrawCollapsible(type, title, heading))
                return;

            EditorGUI.indentLevel++;
            DrawRows(editor, block);
            EditorGUI.indentLevel--;
        }

        // A row is a run of consecutive buttons sharing a row name. Adjacency is safe here because the
        // block was already gathered by name and a row only ever pairs neighbours inside it.
        private static void DrawRows(UnityEditor.Editor editor, List<InspectorButton> block)
        {
            int index = 0;

            while (index < block.Count)
                index = DrawRow(editor, block, index);
        }

        private static int DrawRow(UnityEditor.Editor editor, List<InspectorButton> block, int start)
        {
            string row = block[start].Attribute.Row;

            if (string.IsNullOrEmpty(row))
            {
                DrawOne(editor, block[start]);
                return start + 1;
            }

            int end = start;
            while (end < block.Count && block[end].Attribute.Row == row)
                end++;

            EditorGUILayout.BeginHorizontal();

            for (int i = start; i < end; i++)
                DrawOne(editor, block[i]);

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

        // The title's color is fixed at construction and each constructor takes one kind, so the choice
        // between a hex and a preset is made here rather than by assigning afterwards.
        private static TitleAttribute BuildTitle(ButtonAttribute attribute, string heading)
        {
            TitleAttribute title = string.IsNullOrEmpty(attribute.FoldoutColorHex)
                ? new TitleAttribute(heading, attribute.FoldoutColor)
                : new TitleAttribute(heading, attribute.FoldoutColorHex);

            title.Foldout = true;
            title.DefaultExpanded = attribute.DefaultExpanded;

            return title;
        }

        private static float HeightOf(in InspectorButton button) => button.Attribute.Size == EButtonSize.Large
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