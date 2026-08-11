using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Copies a <see cref="CopyButtonAttribute"/> field value to the clipboard and shows a brief
    /// confirmation. Disabled while the value is empty.
    /// </summary>
    internal sealed class CopyButtonHandler : InlineFieldButtonHandler
    {
        private const float ButtonWidth = 46f;
        private const double NotifyFade = 0.4;
        private const int WidgetOrder = 10;

        private static readonly GUIContent Content = new("Copy", "Copy the value to the clipboard.");

        private static readonly GUIContent Notice = new("Copied");

        protected override int InlineOrder => WidgetOrder;

        protected override float InlineWidth => ButtonWidth;

        protected override bool Applies(in MemberContext context)
            => context.GetAttribute<CopyButtonAttribute>() != null;

        // A string reports isArray = true in Unity because it is a char array, so we must not filter on
        // isArray. Real arrays and lists are Generic, so excluding Generic is enough.
        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType != SerializedPropertyType.Generic;

        protected override bool IsEnabled(in MemberContext context) => !IsEmpty(context.Property);

        protected override GUIContent GetContent(in MemberContext context) => Content;

        protected override void Execute(in MemberContext context) => Copy(context.Property);

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

            if (window == null)
                return;

            window.ShowNotification(Notice, NotifyFade);
        }
    }
}