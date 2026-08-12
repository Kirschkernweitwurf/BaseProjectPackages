using System;
using System.Reflection;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a compact message with a button on its right that runs a repair method.
    /// </summary>
    /// <remarks>
    /// A validation box that only states the problem makes the reader go and solve it by hand, even when
    /// the answer is the same every time: assign the sibling component, point at the one asset of that
    /// type, clear the duplicate. Where the fix is known, the box offers it.
    /// </remarks>
    internal static class FixableHelpBox
    {
        private const float ButtonPadding = 8f;
        private const float MinimumButtonWidth = 40f;

        /// <summary>Draws the message, with a fix button when the method exists.</summary>
        /// <param name="context">The member the message belongs to.</param>
        /// <param name="message">What is wrong.</param>
        /// <param name="type">How badly.</param>
        /// <param name="method">Name of the parameterless repair method, or null for no button.</param>
        /// <param name="label">Label of the button.</param>
        public static void Draw(in MemberContext context, string message, EInfoBoxType type, string method,
            string label)
        {
            MethodInfo repair = Resolve(context, method);

            if (repair == null)
            {
                CompactHelpBox.Draw(message, type);
                return;
            }

            // A local content rather than the shared scratch one, because this is held across the box
            // draw below it and the scratch content is only valid until the next call.
            GUIContent content = new(label);

            float buttonWidth = Mathf.Max(EditorStyles.miniButton.CalcSize(content).x + ButtonPadding,
                MinimumButtonWidth);

            Rect row = EditorGUILayout.GetControlRect(false, CompactHelpBox.Height);
            Rect box = new(row.x, row.y, row.width - buttonWidth - ButtonPadding, row.height);
            Rect button = new(box.xMax + ButtonPadding, row.y, buttonWidth, row.height);

            CompactHelpBox.Draw(box, message, type);

            if (!GUI.Button(button, content, EditorStyles.miniButton))
                return;

            Run(context, repair);
        }

        private static MethodInfo Resolve(in MemberContext context, string method)
        {
            if (string.IsNullOrEmpty(method) || context.DeclaringObject == null)
                return null;

            MethodInfo found = ReflectionCache.GetMethod(context.DeclaringType, method);

            return found != null && found.GetParameters().Length == 0
                ? found
                : null;
        }

        // The fix writes through the object rather than through the SerializedProperty, so pending
        // inspector edits are flushed first and read back afterwards.
        private static void Run(in MemberContext context, MethodInfo method)
        {
            SerializedObject serializedObject = context.Property.serializedObject;

            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(context.Target, method.Name);

            try
            {
                method.Invoke(context.DeclaringObject, null);
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"{method.Name} threw while fixing the field.\n{exception}", context.Target);
            }

            EditorUtility.SetDirty(context.Target);
            serializedObject.Update();
        }
    }
}