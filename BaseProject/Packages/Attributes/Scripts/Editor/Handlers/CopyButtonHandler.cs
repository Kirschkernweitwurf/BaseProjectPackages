using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Copies a <see cref="CopyButtonAttribute"/> field value to the clipboard and shows a brief
    /// confirmation. The button is disabled while the value is empty. Inline by default, or on its own
    /// row when the attribute sets inline to false.
    /// </summary>
    public sealed class CopyButtonHandler : IInlineFieldWidget, IAfterFieldHandler
    {
        private const float InlineWidth = 46f;
        private const double NotifyFade = 0.4;
        private const float RowWidth = 52f;

        private static readonly GUIContent Content = new("Copy", "Copy the value to the clipboard.");
        private static readonly GUIContent Notice = new("Copied");

        int IInlineFieldWidget.Order => 10;
        int IAfterFieldHandler.Order => 90;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<CopyButtonAttribute>() is not { Inline: false })
                return;

            SerializedProperty property = context.Property;
            if (!IsSupported(property))
                return;

            using (new EditorGUI.DisabledScope(IsEmpty(property)))
            {
                if (FieldButtonRenderer.DrawRight(Content, RowWidth))
                    Copy(property);
            }
        }

        public float GetWidth(in MemberContext context)
        {
            CopyButtonAttribute attribute = context.GetAttribute<CopyButtonAttribute>();
            return attribute is { Inline: true } && IsSupported(context.Property)
                ? InlineWidth
                : 0f;
        }

        public void Draw(Rect rect, in MemberContext context)
        {
            SerializedProperty property = context.Property;
            using (new EditorGUI.DisabledScope(IsEmpty(property)))
            {
                if (FieldButtonRenderer.DrawAt(rect, Content))
                    Copy(property);
            }
        }

        // A string reports isArray = true in Unity because it is a char array, so we must not filter on
        // isArray. Real arrays and lists are Generic, so excluding Generic is enough.
        private static bool IsSupported(SerializedProperty property)
            => property.propertyType != SerializedPropertyType.Generic;

        private static bool IsEmpty(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.String)
                return string.IsNullOrEmpty(property.stringValue);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue == null;

            return false;
        }

        private static void Copy(SerializedProperty property)
        {
            EditorGUIUtility.systemCopyBuffer = PropertyValueText.Read(property);

            EditorWindow window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;

            window?.ShowNotification(Notice, NotifyFade);
        }
    }
}