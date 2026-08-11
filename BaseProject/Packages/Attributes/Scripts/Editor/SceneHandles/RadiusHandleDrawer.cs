using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>Draws a draggable circle for <see cref="RadiusHandleAttribute"/>.</summary>
    internal sealed class RadiusHandleDrawer : HandleDrawer<RadiusHandleAttribute>
    {
        protected override void Draw(in HandleContext context, RadiusHandleAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Float)
                return;

            Vector3 position = SceneSpace.Anchor(context, attribute.PositionMember, attribute.Space);
            Vector3 normal = SceneSpace.Normal(context.Transform, attribute.Axis, attribute.Space);
            Quaternion rotation = Quaternion.LookRotation(normal);

            Color previous = UnityEditor.Handles.color;
            UnityEditor.Handles.color = SceneSpace.Resolve(attribute.PresetColor);

            EditorGUI.BeginChangeCheck();
            float radius = UnityEditor.Handles.RadiusHandle(rotation, position, context.Property.floatValue);

            if (EditorGUI.EndChangeCheck())
                context.Property.floatValue = Mathf.Max(0f, radius);

            UnityEditor.Handles.color = previous;
        }
    }
}