using UnityEditor;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Finds the tick count a date or duration row edits. The same row serves a bare <c>long</c> of
    /// ticks and a wrapper struct holding one, so every drawer resolves the property through here
    /// instead of deciding for itself what it was pointed at.
    /// </summary>
    public static class TickProperty
    {
        /// <summary>
        /// Returns the <c>long</c> property holding the ticks, or null when the field is neither a
        /// 64 bit integer nor a struct with the named tick field.
        /// </summary>
        /// <remarks>
        /// A 32 bit integer is rejected rather than widened. Ticks do not fit in one, so accepting it
        /// would write a value back that silently loses the top half of what was typed.
        /// </remarks>
        /// <param name="property">The property the drawer was handed.</param>
        /// <param name="tickField">Name of the serialized tick field inside the wrapper struct.</param>
        /// <returns>The property to edit, or null when there is none.</returns>
        public static SerializedProperty Resolve(SerializedProperty property, string tickField)
        {
            if (property == null)
                return null;

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                return property.numericType == SerializedPropertyNumericType.Int64
                    ? property
                    : null;
            }

            if (property.propertyType != SerializedPropertyType.Generic)
                return null;

            SerializedProperty ticks = property.FindPropertyRelative(tickField);

            return ticks != null && ticks.propertyType == SerializedPropertyType.Integer
                ? ticks
                : null;
        }
    }
}