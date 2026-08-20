using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Reads a numeric bound named by an attribute, for the drawers that take their range from another
    /// member rather than from a constant.
    /// </summary>
    /// <remarks>
    /// A drawer is handed a SerializedProperty and nothing else, so it cannot go through the member
    /// context the handlers use. This resolves against the inspected object instead, which is correct
    /// for a bound: a range that depends on a sibling field belongs to the object, not to the row.
    /// </remarks>
    internal static class BoundResolver
    {
        /// <summary>Reads a single numeric bound from a member.</summary>
        /// <param name="property">The property being drawn.</param>
        /// <param name="member">Name of the member holding the bound.</param>
        /// <param name="value">The resolved bound.</param>
        /// <returns>True when the member resolved to a number.</returns>
        public static bool TryNumber(SerializedProperty property, string member, out float value)
        {
            value = 0f;

            if (!TryRead(property, member, out object read))
                return false;

            switch (read)
            {
                case float floatValue:
                    value = floatValue;
                    return true;
                case int intValue:
                    value = intValue;
                    return true;
                case double doubleValue:
                    value = (float)doubleValue;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>Reads both bounds at once from a Vector2 member.</summary>
        /// <param name="property">The property being drawn.</param>
        /// <param name="member">Name of the Vector2 member holding both bounds.</param>
        /// <param name="min">The resolved lower bound.</param>
        /// <param name="max">The resolved upper bound.</param>
        /// <returns>True when the member resolved to a vector.</returns>
        public static bool TryRange(SerializedProperty property, string member, out float min, out float max)
        {
            min = 0f;
            max = 0f;

            if (!TryRead(property, member, out object read))
                return false;

            switch (read)
            {
                case Vector2 vector:
                    min = vector.x;
                    max = vector.y;
                    return true;
                case Vector2Int vectorInt:
                    min = vectorInt.x;
                    max = vectorInt.y;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryRead(SerializedProperty property, string member, out object value)
        {
            value = null;

            Object target = property.serializedObject.targetObject;

            return target != null && ValueResolver.TryRead(target.GetType(), target, member, out value);
        }
    }
}