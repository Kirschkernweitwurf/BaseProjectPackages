using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws a bool with the checkbox in front of the label for <see cref="LeftToggleAttribute"/>.</summary>
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    public sealed class LeftToggleDrawer : PropertyDrawer
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
