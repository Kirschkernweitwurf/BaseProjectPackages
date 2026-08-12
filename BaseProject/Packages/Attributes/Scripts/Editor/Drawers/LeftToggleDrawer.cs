using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a bool with the checkbox in front of the label for <see cref="LeftToggleAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    internal sealed class LeftToggleDrawer : PropertyDrawer
    {
        private const float LeftPadding = 2f;
        private const float TextGap = 4f;
        private const float ToggleWidth = 14f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<LeftToggleAttribute>("a bool"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Drawn as a box and a label rather than through ToggleLeft, which puts the two flush
            // against each other and the pair hard against the left edge. A small inset lines the box up
            // with the column of labels, and a gap after it stops the text touching the tick.
            // The indent is applied once, to the row, and the two halves are cut out of the result. The
            // controls then run without it, or each of them would add it again.
            Rect row = EditorGUI.IndentedRect(position);
            Rect box = new(row.x + LeftPadding, row.y, ToggleWidth, row.height);
            Rect text = new(box.xMax + TextGap, row.y, row.width - ToggleWidth - LeftPadding - TextGap,
                row.height);

            using (new NoIndentScope())
            {
                property.boolValue = EditorGUI.Toggle(box, property.boolValue);
                EditorGUI.LabelField(text, label);
            }
            EditorGUI.EndProperty();
        }
    }
}