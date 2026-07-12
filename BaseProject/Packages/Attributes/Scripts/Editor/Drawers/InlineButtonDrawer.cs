using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the field with a button next to it for <see cref="InlineButtonAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(InlineButtonAttribute))]
    public sealed class InlineButtonDrawer : PropertyDrawer
    {
        private const float MaxButtonWidth = 140f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InlineButtonAttribute inline = (InlineButtonAttribute)attribute;

            string buttonLabel = string.IsNullOrEmpty(inline.Label)
                ? ObjectNames.NicifyVariableName(inline.Method)
                : inline.Label;

            float buttonWidth =
                Mathf.Min(MaxButtonWidth, GUI.skin.button.CalcSize(new GUIContent(buttonLabel)).x + 10f);

            Rect fieldRect = new(position.x, position.y, position.width - buttonWidth - Spacing, position.height);
            Rect buttonRect = new(fieldRect.xMax + Spacing, position.y, buttonWidth, position.height);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (GUI.Button(buttonRect, buttonLabel))
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