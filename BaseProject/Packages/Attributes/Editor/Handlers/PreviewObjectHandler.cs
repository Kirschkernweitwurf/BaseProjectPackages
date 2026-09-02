using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>
    /// Draws a large preview under a <see cref="PreviewObjectAttribute"/> reference, interactive where
    /// the asset supports it.
    /// </summary>
    /// <remarks>
    /// The interactive preview comes from the asset's own editor, which is what makes a mesh or a prefab
    /// rotatable without this package reimplementing a preview scene. Assets whose editor offers no
    /// interactive preview fall back to the static one, and assets with neither draw nothing rather than
    /// leaving an empty box.
    /// </remarks>
    internal sealed class PreviewObjectHandler : IAfterFieldHandler
    {
        private const int HandlerOrder = 85;
        private const string PreviewKeyPrefix = "PREVIEW";
        private const string PreviewLabel = "Preview";

        /// <inheritdoc/>
        public int Order => HandlerOrder;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            PreviewObjectAttribute attribute = context.GetAttribute<PreviewObjectAttribute>();
            if (attribute == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            Object target = context.Property.objectReferenceValue;
            if (target == null)
                return;

            if (!IsExpanded(context, attribute))
                return;

            Draw(target, attribute);
        }

        private static bool IsExpanded(in MemberContext context, PreviewObjectAttribute attribute)
        {
            if (!attribute.Foldout)
                return true;

            string key = StateKey.For(context.Target.GetType(), PreviewKeyPrefix, context.Property.propertyPath);
            bool stored = EditorPrefs.GetBool(key, attribute.DefaultExpanded);

            EditorGUI.indentLevel++;
            bool expanded = EditorGUILayout.Foldout(stored, PreviewLabel, true);
            EditorGUI.indentLevel--;

            if (expanded != stored)
                EditorPrefs.SetBool(key, expanded);

            return expanded;
        }

        private static void Draw(Object target, PreviewObjectAttribute attribute)
        {
            Rect rect = Reserve(attribute);

            UnityEditor.Editor editor = EmbeddedEditorCache.Get(target);

            if (attribute.Interactive && editor != null && editor.HasPreviewGUI())
            {
                editor.OnInteractivePreviewGUI(rect, GUIStyle.none);
                return;
            }

            // Null-coalescing would use the plain C# check and miss Unity's fake null, so the fallback
            // is written out.
            Texture2D still = AssetPreview.GetAssetPreview(target);

            if (still == null)
                still = AssetPreview.GetMiniThumbnail(target);

            if (still != null)
                GUI.DrawTexture(rect, still, ScaleMode.ScaleToFit);
        }

        private static Rect Reserve(PreviewObjectAttribute attribute)
        {
            Rect row = EditorGUILayout.GetControlRect(false, attribute.Height);
            row = EditorGUI.IndentedRect(row);

            if (attribute.Width <= 0f)
                return row;

            return new Rect(row.x, row.y, Mathf.Min(attribute.Width, row.width), row.height);
        }
    }
}