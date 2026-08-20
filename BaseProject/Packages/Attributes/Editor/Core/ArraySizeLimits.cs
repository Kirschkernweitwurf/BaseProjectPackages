using UnityEditor;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Reads the bounds an <see cref="ArraySizeAttribute"/> puts on a field. Shared so the handler that
    /// enforces them and the collection renderers that hide their add and remove buttons cannot end up
    /// disagreeing about what the limits are.
    /// </summary>
    internal static class ArraySizeLimits
    {
        /// <summary>Returns the bounds on the given member, or false when it has none.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <param name="minimum">Smallest allowed count, or -1.</param>
        /// <param name="maximum">Largest allowed count, or -1.</param>
        /// <returns>True when the member is bounded and is an array.</returns>
        public static bool TryGet(in MemberContext context, out int minimum, out int maximum)
        {
            minimum = ArraySizeAttribute.Unbounded;
            maximum = ArraySizeAttribute.Unbounded;

            ArraySizeAttribute attribute = context.GetAttribute<ArraySizeAttribute>();
            if (attribute == null)
                return false;

            if (!context.Property.isArray || context.Property.propertyType == SerializedPropertyType.String)
                return false;

            if (attribute.Size >= 0)
            {
                minimum = attribute.Size;
                maximum = attribute.Size;
                return true;
            }

            minimum = attribute.Min;
            maximum = attribute.Max;

            return minimum >= 0 || maximum >= 0;
        }

        /// <summary>
        /// Returns whether the member's element count can still be changed from the inspector, which is
        /// what decides if the add and remove controls are worth drawing.
        /// </summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when rows may be added or removed.</returns>
        public static bool CanResize(in MemberContext context)
        {
            if (!TryGet(context, out int minimum, out int maximum))
                return true;

            return minimum < 0 || maximum < 0 || minimum != maximum;
        }
    }
}