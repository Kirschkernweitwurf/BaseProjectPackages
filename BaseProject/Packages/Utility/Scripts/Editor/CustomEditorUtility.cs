using UnityEditor;

namespace Base.UtilityPackage.Editor
{
    /// <summary>
    /// Utility class for editor-related helper methods.
    /// </summary>
    public static class CustomEditorUtility
    {
        /// <summary>
        /// Finds a SerializedProperty by its nice name or backing field name.
        /// </summary>
        /// <param name="so">The SerializedObject to search in.</param>
        /// <param name="niceName">
        /// The nice name of the property.
        /// Meaning the actual field name without compiler-generated backing field syntax.
        /// </param>
        /// <returns>The found SerializedProperty, or <c>null</c> if not found.</returns>
        public static SerializedProperty FindProp(SerializedObject so, string niceName)
            => so.FindProperty(niceName) ?? so.FindProperty($"<{niceName}>k__BackingField");

        /// <summary>
        /// Finds a nested SerializedProperty by its nice name or backing field name,
        /// relative to a parent SerializedProperty.
        /// </summary>
        /// <param name="parent">The parent SerializedProperty to search within.</param>
        /// <param name="niceName">
        /// The nice name of the property.
        /// Meaning the actual field name without compiler-generated backing field syntax.
        /// </param>
        /// <returns>The found SerializedProperty, or <c>null</c> if not found.</returns>
        public static SerializedProperty FindProp(SerializedProperty parent, string niceName)
            => parent.FindPropertyRelative(niceName) ?? parent.FindPropertyRelative($"<{niceName}>k__BackingField");
    }
}