using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws the field with a button next to it for <see cref="InlineButtonAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(InlineButtonAttribute))]
    internal sealed class InlineButtonDrawer : PropertyDrawer
    {
        private const float LabelPadding = 10f;
        private const float MaxButtonWidth = 140f;
        private const float Spacing = 2f;

        private string _buttonLabel;
        private float _buttonWidth = -1f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InlineButtonAttribute inline = (InlineButtonAttribute)attribute;

            _buttonLabel ??= string.IsNullOrEmpty(inline.Label)
                ? ObjectNames.NicifyVariableName(inline.Method)
                : inline.Label;

            if (_buttonWidth < 0f)
            {
                float textWidth = GUI.skin.button.CalcSize(ScratchContent.For(_buttonLabel)).x;
                _buttonWidth = Mathf.Min(MaxButtonWidth, textWidth + LabelPadding);
            }

            Rect fieldRect = new(position.x, position.y, position.width - _buttonWidth - Spacing, position.height);
            Rect buttonRect = new(fieldRect.xMax + Spacing, position.y, _buttonWidth, position.height);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (GUI.Button(buttonRect, _buttonLabel))
                Invoke(property, inline.Method);
        }

        private static void Invoke(SerializedProperty property, string methodName)
        {
            foreach (Object target in property.serializedObject.targetObjects)
            {
                MethodInfo method = ReflectionCache.GetMethod(target.GetType(), methodName);
                if (method != null && method.GetParameters().Length == 0)
                    method.Invoke(target, null);
            }
        }
    }
}