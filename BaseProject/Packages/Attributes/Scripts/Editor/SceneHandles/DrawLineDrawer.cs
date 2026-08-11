using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>Draws a connecting line for <see cref="DrawLineAttribute"/>.</summary>
    internal sealed class DrawLineDrawer : HandleDrawer<DrawLineAttribute>
    {
        private const float DotSpacing = 3f;

        protected override void Draw(in HandleContext context, DrawLineAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Vector3)
                return;

            Vector3 from = SceneSpace.Anchor(context, attribute.FromMember, attribute.Space);
            Vector3 to = SceneSpace.ToWorld(context.Transform, context.Property.vector3Value, attribute.Space);

            Color previous = UnityEditor.Handles.color;
            UnityEditor.Handles.color = SceneSpace.Resolve(attribute.PresetColor);

            if (attribute.Dotted)
                UnityEditor.Handles.DrawDottedLine(from, to, DotSpacing);
            else
                UnityEditor.Handles.DrawLine(from, to);

            UnityEditor.Handles.color = previous;
        }
    }
}