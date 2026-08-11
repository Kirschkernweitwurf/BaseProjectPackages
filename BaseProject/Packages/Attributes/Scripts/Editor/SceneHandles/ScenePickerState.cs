using UnityEditor;

namespace Base.AttributePackage.Editor.SceneHandles
{
    /// <summary>
    /// Remembers which field is currently waiting for a scene view click. Only one field can be armed at
    /// a time, because the next click has to belong to exactly one of them.
    /// </summary>
    public static class ScenePickerState
    {
        private static int _targetId;
        private static string _propertyPath;

        /// <summary>True while some field is waiting for a click.</summary>
        public static bool IsArmed => _propertyPath != null;

        /// <summary>Arms the given property, replacing whatever was armed before.</summary>
        /// <param name="property">The object reference property to fill on the next click.</param>
        public static void Arm(SerializedProperty property)
        {
            _targetId = property.serializedObject.targetObject.GetInstanceID();
            _propertyPath = property.propertyPath;
        }

        /// <summary>Clears the armed state.</summary>
        public static void Disarm()
        {
            _targetId = 0;
            _propertyPath = null;
        }

        /// <summary>Returns whether the given property is the one waiting for a click.</summary>
        /// <param name="property">The property to test.</param>
        /// <returns>True when this property is armed.</returns>
        public static bool IsArmedFor(SerializedProperty property)
        {
            if (_propertyPath == null)
                return false;

            return _propertyPath == property.propertyPath
                && _targetId == property.serializedObject.targetObject.GetInstanceID();
        }
    }
}
