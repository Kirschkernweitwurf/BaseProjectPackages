using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Draws a thumbnail for <see cref="ShowAssetPreviewAttribute"/> references.</summary>
    public sealed class AssetPreviewHandler : IAfterFieldHandler
    {
        public int Order => 100;

        public void AfterField(in MemberContext context)
        {
            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            ShowAssetPreviewAttribute attribute = context.GetAttribute<ShowAssetPreviewAttribute>();
            if (attribute == null)
                return;

            Object value = context.Property.objectReferenceValue;
            if (value == null)
                return;

            Texture2D texture = AssetPreview.GetAssetPreview(value);
            if (texture == null)
            {
                if (AssetPreview.IsLoadingAssetPreview(value.GetInstanceID()))
                    context.Editor.Repaint();

                return;
            }

            int size = Mathf.Clamp(attribute.Size, 16, 512);
            GUILayout.Label(texture, GUILayout.Width(size), GUILayout.Height(size));
        }
    }
}