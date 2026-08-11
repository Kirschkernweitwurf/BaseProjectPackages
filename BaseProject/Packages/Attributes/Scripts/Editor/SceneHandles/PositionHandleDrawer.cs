using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>Draws a movable position gizmo for <see cref="PositionHandleAttribute"/>.</summary>
    internal sealed class PositionHandleDrawer : HandleDrawer<PositionHandleAttribute>
    {
        private static readonly Vector3 LabelOffset = new(0f, 0.15f, 0f);

        protected override void Draw(in HandleContext context, PositionHandleAttribute attribute)
        {
            if (context.Property.propertyType != SerializedPropertyType.Vector3)
                return;

            Vector3 world = SceneSpace.ToWorld(context.Transform, context.Property.vector3Value, attribute.Space);

            EditorGUI.BeginChangeCheck();
            Vector3 moved = UnityEditor.Handles.PositionHandle(world,
                SceneSpace.Rotation(context.Transform, attribute.Space));

            if (EditorGUI.EndChangeCheck())
                context.Property.vector3Value = SceneSpace.ToStored(context.Transform, moved, attribute.Space);

            string label = attribute.Label ?? context.DisplayName;
            UnityEditor.Handles.Label(moved + LabelOffset, label);
        }
    }
}