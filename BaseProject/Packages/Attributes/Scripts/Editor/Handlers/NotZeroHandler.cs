using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Pushes <see cref="NotZeroAttribute"/> fields away from zero. Applies to int and float and to each
    /// component of Vector2, Vector3, Vector2Int and Vector3Int. The last non-zero value per field is
    /// remembered, so a value that reaches zero returns to the side it came from and keeps its sign.
    /// </summary>
    public sealed class NotZeroHandler : IAfterFieldHandler
    {
        private const string KeySeparator = ".";

        private static readonly Dictionary<string, float> Previous = new();

        public int Order => 10;

        public void AfterField(in MemberContext context)
        {
            NotZeroAttribute attribute = context.GetAttribute<NotZeroAttribute>();
            if (attribute == null)
                return;

            SerializedProperty property = context.Property;
            float step = Mathf.Abs(attribute.Step);
            if (Mathf.Approximately(step, 0f))
                step = 1f;

            string path = property.propertyPath;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    int stepInt = Mathf.Max(1, Mathf.RoundToInt(step));
                    property.intValue = Mathf.RoundToInt(Resolve(path, property.intValue, stepInt));
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = Resolve(path, property.floatValue, step);
                    break;

                case SerializedPropertyType.Vector2:
                    Vector2 vector2 = property.vector2Value;
                    vector2.x = Resolve(Key(path, 0), vector2.x, step);
                    vector2.y = Resolve(Key(path, 1), vector2.y, step);
                    property.vector2Value = vector2;
                    break;

                case SerializedPropertyType.Vector3:
                    Vector3 vector3 = property.vector3Value;
                    vector3.x = Resolve(Key(path, 0), vector3.x, step);
                    vector3.y = Resolve(Key(path, 1), vector3.y, step);
                    vector3.z = Resolve(Key(path, 2), vector3.z, step);
                    property.vector3Value = vector3;
                    break;

                case SerializedPropertyType.Vector2Int:
                    int step2Int = Mathf.Max(1, Mathf.RoundToInt(step));
                    Vector2Int vector2Int = property.vector2IntValue;
                    vector2Int.x = Mathf.RoundToInt(Resolve(Key(path, 0), vector2Int.x, step2Int));
                    vector2Int.y = Mathf.RoundToInt(Resolve(Key(path, 1), vector2Int.y, step2Int));
                    property.vector2IntValue = vector2Int;
                    break;

                case SerializedPropertyType.Vector3Int:
                    int step3Int = Mathf.Max(1, Mathf.RoundToInt(step));
                    Vector3Int vector3Int = property.vector3IntValue;
                    vector3Int.x = Mathf.RoundToInt(Resolve(Key(path, 0), vector3Int.x, step3Int));
                    vector3Int.y = Mathf.RoundToInt(Resolve(Key(path, 1), vector3Int.y, step3Int));
                    vector3Int.z = Mathf.RoundToInt(Resolve(Key(path, 2), vector3Int.z, step3Int));
                    property.vector3IntValue = vector3Int;
                    break;
            }
        }

        private static string Key(string path, int component) => path + KeySeparator + component;

        private static float Resolve(string key, float value, float step)
        {
            if (!Mathf.Approximately(value, 0f))
            {
                Previous[key] = value;
                return value;
            }

            // Zero is not allowed. Keep the value on the side it came from, so a field that was
            // negative stays negative and a field that was positive stays positive.
            float pushed = Previous.TryGetValue(key, out float previous) && previous < 0f
                ? -step
                : step;

            Previous[key] = pushed;
            return pushed;
        }
    }
}