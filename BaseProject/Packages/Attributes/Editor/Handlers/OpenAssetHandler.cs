using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Opens the referenced asset for <see cref="OpenAssetAttribute"/> fields. Works on object
    /// references and on string fields holding a project asset path. Disabled when nothing is assigned.
    /// </summary>
    internal sealed class OpenAssetHandler : InlineFieldButtonHandler
    {
        private const float ButtonWidth = 46f;
        private const string DefaultLabel = "Open";
        private const string Tooltip = "Open the asset.";
        private const int WidgetOrder = 20;

        protected override int InlineOrder => WidgetOrder;

        protected override float InlineWidth => ButtonWidth;

        private static readonly GUIContent Content = new(DefaultLabel, Tooltip);

        // Reused so a custom label does not allocate a new content every repaint.
        private static readonly GUIContent CustomContent = new(string.Empty, Tooltip);

        protected override bool Applies(in MemberContext context)
            => context.GetAttribute<OpenAssetAttribute>() != null;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
                || property.propertyType == SerializedPropertyType.String;

        protected override bool IsEnabled(in MemberContext context) => Resolve(context.Property) != null;

        protected override GUIContent GetContent(in MemberContext context)
        {
            OpenAssetAttribute attribute = context.GetAttribute<OpenAssetAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.Label))
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