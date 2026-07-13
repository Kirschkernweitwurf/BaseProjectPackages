using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clamps <see cref="MaxAttribute"/> fields to a maximum. Applies to int and float and to each
    /// component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    public sealed class MaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MaxAttribute attribute = context.GetAttribute<MaxAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;
            float max = attribute.Max;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    int maxInt = Mathf.RoundToInt(max);
                    if (property.intValue > maxInt)
                        property.intValue = maxInt;
                    break;

                case SerializedPropertyType.Float:
                    if (property.floatValue > max)
                        property.floatValue = max;
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = ClampMax(property.vector2Value, max);
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = ClampMax(property.vector3Value, max);
                    break;

                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = ClampMax(property.vector2IntValue, Mathf.RoundToInt(max));
                    break;

                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = ClampMax(property.vector3IntValue, Mathf.RoundToInt(max));
                    break;
            }
        }

        private static Vector2 ClampMax(Vector2 value, float max)
            => new(Mathf.Min(value.x, max), Mathf.Min(value.y, max));

        private static Vector3 ClampMax(Vector3 value, float max)
            => new(Mathf.Min(value.x, max), Mathf.Min(value.y, max), Mathf.Min(value.z, max));

        private static Vector2Int ClampMax(Vector2Int value, int max)
            => new(Mathf.Min(value.x, max), Mathf.Min(value.y, max));

        private static Vector3Int ClampMax(Vector3Int value, int max)
            => new(Mathf.Min(value.x, max), Mathf.Min(value.y, max), Mathf.Min(value.z, max));
    }
}
