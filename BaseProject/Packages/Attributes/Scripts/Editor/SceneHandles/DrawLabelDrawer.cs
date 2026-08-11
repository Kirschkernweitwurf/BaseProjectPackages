using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>Draws floating text for <see cref="DrawLabelAttribute"/>.</summary>
    internal sealed class DrawLabelDrawer : HandleDrawer<DrawLabelAttribute>
    {
        protected override void Draw(in HandleContext context, DrawLabelAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Vector3)
                return;

            Vector3 position = SceneSpace.ToWorld(context.Transform, context.Property.vector3Value,
                attribute.Space);

            GUIStyle style = new(EditorStyles.label)
            {
                normal =
                {
                    textColor = SceneSpace.Resolve(attribute.PresetColor)
                }
            };

            UnityEditor.Handles.Label(position, ResolveText(context, attribute), style);
        }

        // A member wins over the constant text, so a label can show a live value rather than a caption.
        private static string ResolveText(in HandleContext context, DrawLabelAttribute attribute)
        {
            if (context.TryResolveText(attribute.TextMember, out string resolved))
                return resolved;

            return attribute.Text ?? context.DisplayName;
        }
    }
}