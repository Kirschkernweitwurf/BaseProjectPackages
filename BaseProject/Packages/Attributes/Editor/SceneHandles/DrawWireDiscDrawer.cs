using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.SceneHandles
{
    /// <summary>Draws a non-interactive wire circle for <see cref="DrawWireDiscAttribute"/>.</summary>
    internal sealed class DrawWireDiscDrawer : HandleDrawer<DrawWireDiscAttribute>
    {
        protected override void Draw(in HandleContext context, DrawWireDiscAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Float)
                return;

            Vector3 position = SceneSpace.Anchor(context, attribute.PositionMember, attribute.Space);
            Vector3 normal = SceneSpace.Normal(context.Transform, attribute.Axis, attribute.Space);

            Color previous = Handles.color;
            Handles.color = SceneSpace.Resolve(attribute.PresetColor);

            Handles.DrawWireDisc(position, normal, context.Property.floatValue);

            Handles.color = previous;
        }
    }
}