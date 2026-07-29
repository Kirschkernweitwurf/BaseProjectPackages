using UnityEditor;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Utility class for editor-related helper methods.
    /// </summary>
    public static class CustomEditorUtility
    {
        // Layout the compiler uses for the hidden field behind an auto-implemented property.
        private const string BackingFieldFormat = "<{0}>k__BackingField";

        /// <summary>
        /// Finds a <see cref="SerializedProperty"/> by its nice name or backing field name.
        /// </summary>
        /// <param name="serializedObject">The object to search in.</param>
        /// <param name="niceName">
        /// The nice name of the property.
        /// Meaning the actual field name without compiler-generated backing field syntax.
        /// </param>
        /// <returns>The found property, or <c>null</c> if not found.</returns>
        public static SerializedProperty FindProp(SerializedObject serializedObject, string niceName)
            => serializedObject.FindProperty(niceName)
                ?? serializedObject.FindProperty(string.Format(BackingFieldFormat, niceName));

        /// <summary>
        /// Finds a nested <see cref="SerializedProperty"/> by its nice name or backing field name,
        /// relative to a parent property.
        /// </summary>
        /// <param name="parent">The parent property to search within.</param>
        /// <param name="niceName">
        /// The nice name of the property.
        /// Meaning the actual field name without compiler-generated backing field syntax.
        /// </param>
        /// <returns>The found property, or <c>null</c> if not found.</returns>
        public static SerializedProperty FindProp(SerializedProperty parent, string niceName)
            => parent.FindPropertyRelative(niceName)
                ?? parent.FindPropertyRelative(string.Format(BackingFieldFormat, niceName));
    }
}