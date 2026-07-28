using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Opens the referenced asset for <see cref="OpenAssetAttribute"/> fields. Works on object
    /// references and on string fields holding a project asset path. Disabled when nothing is assigned.
    /// Inline by default, or on its own row when inline is false.
    /// </summary>
    public sealed class OpenAssetHandler : FieldButtonHandler
    {
        private const int AfterFieldOrder = 92;
        private const string DefaultLabel = "Open";
        private const float InlineButtonWidth = 46f;
        private const float RowButtonWidth = 60f;
        private const string Tooltip = "Open the asset.";
        private const int WidgetOrder = 20;

        private static readonly GUIContent Content = new(DefaultLabel, Tooltip);

        // Reused so a custom row label does not allocate a new content every repaint.
        private static readonly GUIContent CustomContent = new(string.Empty, Tooltip);

        protected override int InlineOrder => WidgetOrder;

        protected override int RowOrder => AfterFieldOrder;

        protected override float InlineWidth => InlineButtonWidth;

        protected override float RowWidth => RowButtonWidth;

        protected override bool TryGetPlacement(in MemberContext context, out bool inline)
        {
            OpenAssetAttribute attribute = context.GetAttribute<OpenAssetAttribute>();
            inline = attribute != null && attribute.Inline;
            return attribute != null;
        }

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
                || property.propertyType == SerializedPropertyType.String;

        protected override bool IsEnabled(in MemberContext context) => Resolve(context.Property) != null;

        protected override GUIContent GetContent(in MemberContext context)
        {
            OpenAssetAttribute attribute = context.GetAttribute<OpenAssetAttribute>();
            if (attribute == null || attribute.Inline || string.IsNullOrEmpty(attribute.Label))
                return Content;

            CustomContent.text = attribute.Label;
            return CustomContent;
        }

        protected override void Execute(in MemberContext context)
        {
            Object asset = Resolve(context.Property);
            if (asset == null)
                return;

            AssetDatabase.OpenAsset(asset);
        }

        private static Object Resolve(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
                return property.objectReferenceValue;

            if (property.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(property.stringValue))
                return AssetDatabase.LoadAssetAtPath<Object>(property.stringValue);

            return null;
        }
    }
}