using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Draws a rotation gizmo for <see cref="RotationHandleAttribute"/>. Accepts a Quaternion field or a
    /// Vector3 field holding euler angles, since both are common ways to author a rotation.
    /// </summary>
    internal sealed class RotationHandleDrawer : HandleDrawer<RotationHandleAttribute>
    {
        protected override void Draw(in HandleContext context, RotationHandleAttribute attribute)
        {
            SerializedProperty property = context.Property;
            bool isEuler = property.propertyType == SerializedPropertyType.Vector3;

            if (!isEuler && property.propertyType != SerializedPropertyType.Quaternion)
                return;

            Quaternion stored = isEuler
                ? Quaternion.Euler(property.vector3Value)
                : property.quaternionValue;

            Quaternion parent = SceneSpace.Rotation(context.Transform, attribute.Space);
            Vector3 position = SceneSpace.Anchor(context, attribute.PositionMember, attribute.Space);

            EditorGUI.BeginChangeCheck();
            Quaternion rotated = Handles.RotationHandle(parent * stored, position);

            if (!EditorGUI.EndChangeCheck())
                return;

            Quaternion result = Quaternion.Inverse(parent) * rotated;

            if (isEuler)
                property.vector3Value = result.eulerAngles;
            else
                property.quaternionValue = result;
        }
    }
}