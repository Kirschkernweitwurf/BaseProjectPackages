using Base.AttributePackage.Conditional;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws fields marked with <see cref="ReadOnlyAttribute"/> as disabled.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousState = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = previousState;
        }
    }
}