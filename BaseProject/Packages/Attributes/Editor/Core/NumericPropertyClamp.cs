using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Clamps a numeric serialized property into a range. Applies to int and float and component wise
    /// to Vector2, Vector3, Vector2Int and Vector3Int, mirroring how Unity's own <c>[Min]</c> handles
    /// vectors. Shared by the min and max handlers, so both clamp identically.
    /// </summary>
    internal static class NumericPropertyClamp
    {
        /// <summary>
        /// Clamps the property into the inclusive range. Pass an infinity for a side that is unbounded.
        /// </summary>
        public static void Apply(SerializedProperty property, float min, float max)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    int clampedInt = ToInt(property.intValue, min, max);
                    if (clampedInt != property.intValue)
                        property.intValue = clampedInt;

                    break;

                case SerializedPropertyType.Float:
                    float clamped = Mathf.Clamp(property.floatValue, min, max);
                    if (!Mathf.Approximately(clamped, property.floatValue))
                        property.floatValue = clamped;

                    break;

                case SerializedPropertyType.Vector2:
                    Vector2 vector2 = property.vector2Value;
                    property.vector2Value =
                        new Vector2(Mathf.Clamp(vector2.x, min, max), Mathf.Clamp(vector2.y, min, max));

                    break;

                case SerializedPropertyType.Vector3:
                    Vector3 vector3 = property.vector3Value;
                    property.vector3Value = new Vector3(Mathf.Clamp(vector3.x, min, max),
                        Mathf.Clamp(vector3.y, min, max), Mathf.Clamp(vector3.z, min, max));

                    break;

                case SerializedPropertyType.Vector2Int:
                    Vector2Int vector2Int = property.vector2IntValue;
                    property.vector2IntValue =
                        new Vector2Int(ToInt(vector2Int.x, min, max), ToInt(vector2Int.y, min, max));

                    break;

                case SerializedPropertyType.Vector3Int:
                    Vector3Int vector3Int = property.vector3IntValue;
                    property.vector3IntValue = new Vector3Int(ToInt(vector3Int.x, min, max),
                        ToInt(vector3Int.y, min, max), ToInt(vector3Int.z, min, max));

                    break;
            }
        }

        private static int ToInt(int value, float min, float max) => Mathf.RoundToInt(Mathf.Clamp(value, min, max));
    }
}