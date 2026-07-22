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
    public sealed class OpenAssetHandler : IInlineFieldWidget, IAfterFieldHandler
    {
        private const string DefaultLabel = "Open";
        private const float InlineWidth = 46f;
        private const float RowWidth = 60f;

        int IInlineFieldWidget.Order => 20;

        int IAfterFieldHandler.Order => 92;

        private static readonly GUIContent Content = new(DefaultLabel, "Open the asset.");

        public void AfterField(in MemberContext context)
        {
            OpenAssetAttribute attribute = context.GetAttribute<OpenAssetAttribute>();
            if (attribute is not
                {
                    Inline: false
                })
                return;

            Object asset = Resolve(context.Property);
            GUIContent content = new(attribute.Label ?? DefaultLabel, "Open the asset.");

            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (FieldButtonRenderer.DrawRight(content, RowWidth) && asset != null)
                    AssetDatabase.OpenAsset(asset);
            }
        }

        public float GetWidth(in MemberContext context)
        {
            OpenAssetAttribute attribute = context.GetAttribute<OpenAssetAttribute>();
            return attribute is
                {
                    Inline: true
                }
                && IsSupported(context.Property)
                    ? InlineWidth
                    : 0f;
        }

        public void Draw(Rect rect, in MemberContext context)
        {
            Object asset = Resolve(context.Property);
            using (new EditorGUI.DisabledScope(asset == null))
            {
                if (FieldButtonRenderer.DrawAt(rect, Content) && asset != null)
                    AssetDatabase.OpenAsset(asset);
            }
        }

        private static bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.ObjectReference
                || property.propertyType == SerializedPropertyType.String;

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