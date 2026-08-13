using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the checkbox of a <see cref="PrefixToggleAttribute"/> in the gutter in front of the field's
    /// label. Runs first among the after-field handlers so the rect it draws over is still the field's
    /// own row.
    /// </summary>
    internal sealed class PrefixToggleHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = -190;

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            SerializedProperty toggle = PrefixToggleState.ResolveToggle(context);
            if (toggle == null)
                return;

            // The rect from the layout pass is a placeholder, so there is nothing to draw over yet.
            if (Event.current.type == EventType.Layout)
                return;

            Rect row = GUILayoutUtility.GetLastRect();
            Rect box = LeadingGutter.SquareFor(row, EditorGUI.indentLevel,
                EditorGUIUtility.singleLineHeight);

            bool stored = toggle.boolValue;
            bool value;

            // The gutter rect already accounts for the indent, so letting the control apply it a second
            // time would push the checkbox one step off its own field.
            using (new NoIndentScope())
                value = EditorGUI.Toggle(box, stored);

            if (value != stored)
                toggle.boolValue = value;
        }
    }
}