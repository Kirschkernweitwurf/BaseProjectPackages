using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Clamps <see cref="MinMaxAttribute"/> fields into an inclusive range. Applies to int and float
    /// and to each component of Vector2, Vector3, Vector2Int and Vector3Int.
    /// </summary>
    public sealed class MinMaxHandler : IAfterFieldHandler
    {
        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            MinMaxAttribute attribute = context.GetAttribute<MinMaxAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;
            float min = attribute.Min;
            float max = attribute.Max;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    int clampedInt = Mathf.Clamp(property.intValue, Mathf.RoundToInt(min), Mathf.RoundToInt(max));
                    if (clampedInt != property.intValue)
                        property.intValue = clampedInt;

                    break;

                case SerializedPropertyType.Float:
                    float clamped = Mathf.Clamp(property.floatValue, min, max);
                    if (!Mathf.Approximately(clamped, property.floatValue))
                        property.floatValue = clamped;

                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = Clamp(property.vector2Value, min, max);
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = Clamp(property.vector3Value, min, max);
                    break;

                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue =
                        Clamp(property.vector2IntValue, Mathf.RoundToInt(min), Mathf.RoundToInt(max));

                    break;

                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue =
                        Clamp(property.vector3IntValue, Mathf.RoundToInt(min), Mathf.RoundToInt(max));

                    break;
            }
        }

        private static Vector2 Clamp(Vector2 value, float min, float max)
            => new(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));

        private static Vector3 Clamp(Vector3 value, float min, float max) => new(Mathf.Clamp(value.x, min, max),
            Mathf.Clamp(value.y, min, max), Mathf.Clamp(value.z, min, max));

        private static Vector2Int Clamp(Vector2Int value, int min, int max)
            => new(Mathf.Clamp(value.x, min, max), Mathf.Clamp(value.y, min, max));

        private static Vector3Int Clamp(Vector3Int value, int min, int max) => new(Mathf.Clamp(value.x, min, max),
            Mathf.Clamp(value.y, min, max), Mathf.Clamp(value.z, min, max));
    }
}