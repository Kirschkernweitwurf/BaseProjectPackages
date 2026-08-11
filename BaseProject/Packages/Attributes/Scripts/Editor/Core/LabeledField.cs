using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a labeled control while keeping the label's tooltip.
    /// </summary>
    /// <remarks>
    /// The convenient EditorGUI overloads take the label as a plain string, which silently throws away
    /// the tooltip Unity resolved from the field's <see cref="TooltipAttribute"/>. The overloads that do
    /// keep it want the options as GUIContent too, which would mean allocating an array every repaint.
    /// Drawing the prefix label separately avoids both, so every drawer in the package goes through here
    /// rather than reaching for the string overload again.
    /// </remarks>
    public static class LabeledField
    {
        /// <summary>Draws a dropdown that keeps the label's tooltip.</summary>
        /// <param name="rect">The full field row.</param>
        /// <param name="label">The label, tooltip included.</param>
        /// <param name="selected">The currently selected index.</param>
        /// <param name="options">The option labels.</param>
        /// <returns>The index the user picked.</returns>
        public static int Popup(Rect rect, GUIContent label, int selected, string[] options)
        {
            Rect field = Prefix(rect, label);

            // The prefix label already consumed the indent, so the control must not apply it a second
            // time or it walks further right on every nesting level.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            int result = EditorGUI.Popup(field, selected, options);

            EditorGUI.indentLevel = indent;
            return result;
        }

        /// <summary>Draws a read-only hint in place of a value, keeping the label's tooltip.</summary>
        /// <param name="rect">The full field row.</param>
        /// <param name="label">The label, tooltip included.</param>
        /// <param name="message">The hint shown where the value would be.</param>
        public static void Hint(Rect rect, GUIContent label, string message)
            => EditorGUI.LabelField(rect, label, new GUIContent(message, label.tooltip));

        /// <summary>Draws the label and returns the rect the control should fill.</summary>
        /// <param name="rect">The full field row.</param>
        /// <param name="label">The label, tooltip included.</param>
        /// <returns>The remaining rect for the control.</returns>
        public static Rect Prefix(Rect rect, GUIContent label) => EditorGUI.PrefixLabel(rect, label);
    }
}
