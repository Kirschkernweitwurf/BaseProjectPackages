using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.SceneHandles
{
    /// <summary>Draws a scale gizmo for <see cref="ScaleHandleAttribute"/>.</summary>
    internal sealed class ScaleHandleDrawer : HandleDrawer<ScaleHandleAttribute>
    {
        protected override void Draw(in HandleContext context, ScaleHandleAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Vector3)
                return;

            Vector3 position = SceneSpace.Anchor(context, attribute.PositionMember, ESpace.Local);
            Quaternion rotation = SceneSpace.Rotation(context.Transform, ESpace.Local);

            EditorGUI.BeginChangeCheck();
            Vector3 scaled = Handles.ScaleHandle(context.Property.vector3Value, position, rotation,
                attribute.Size);

            if (EditorGUI.EndChangeCheck())
                context.Property.vector3Value = scaled;
        }
    }
}