using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.SceneHandles
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

            Color previous = Handles.color;
            Handles.color = SceneSpace.Resolve(attribute.PresetColor);

            if (attribute.Dotted)
                Handles.DrawDottedLine(from, to, DotSpacing);
            else
                Handles.DrawLine(from, to);

            Handles.color = previous;
        }
    }
}