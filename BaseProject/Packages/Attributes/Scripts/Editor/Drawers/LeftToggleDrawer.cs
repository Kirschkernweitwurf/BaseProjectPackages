using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a <see cref="LeftToggleAttribute"/> bool with its checkbox in front of the label.
    /// </summary>
    /// <remarks>
    /// Unity's own control, not a box and a label placed by hand. It already spaces the two the way
    /// every other left-aligned toggle in the editor is spaced, and reproducing that by hand only
    /// produced something close enough to look wrong beside the real thing.
    /// <para>
    /// <see cref="LeadingGutter"/> is lined up against this rather than the other way round, so a prefix
    /// toggle and a left toggle sitting next to each other share an edge.
    /// </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    internal sealed class LeftToggleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Boolean)
            {
                LabeledField.Hint(position, label, AttributeNames.Usage<LeftToggleAttribute>("a bool"));
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            property.boolValue = EditorGUI.ToggleLeft(position, label, property.boolValue);
            EditorGUI.EndProperty();
        }
    }
}