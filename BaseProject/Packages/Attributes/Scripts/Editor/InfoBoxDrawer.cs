using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the help box for <see cref="InfoBoxAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public sealed class InfoBoxDrawer : DecoratorDrawer
    {
        private const float BottomSpacing = 4f;
        private const float IconWidth = 28f;
        private const float MinHeight = 30f;
        private const float TopSpacing = 4f;

        public override float GetHeight()
        {
            InfoBoxAttribute box = (InfoBoxAttribute)attribute;

            float width = EditorGUIUtility.currentViewWidth - IconWidth - 24f;
            if (width < 40f)
                width = 200f;

            GUIContent content = new(box.Message);
            float textHeight = EditorStyles.helpBox.CalcHeight(content, width);

            return Mathf.Max(textHeight, MinHeight) + TopSpacing + BottomSpacing;
        }

        public override void OnGUI(Rect position)
        {
            InfoBoxAttribute box = (InfoBoxAttribute)attribute;

            Rect boxRect = new(position.x,
                position.y + TopSpacing,
                position.width,
                position.height - TopSpacing - BottomSpacing);

            EditorGUI.HelpBox(boxRect, box.Message, ToMessageType(box.Type));
        }

        private static MessageType ToMessageType(EInfoBoxType type)
        {
            switch (type)
            {
                case EInfoBoxType.Info:
                    return MessageType.Info;
                case EInfoBoxType.Warning:
                    return MessageType.Warning;
                case EInfoBoxType.Error:
                    return MessageType.Error;
                default:
                    return MessageType.None;
            }
        }
    }
}