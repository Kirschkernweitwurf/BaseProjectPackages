#if UNITY_EDITOR
using UnityEditor;

namespace Utility.Editor
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
        /// <param name="niceName">The nice name of the property.
        /// Meaning the actual field name without compiler-generated backing field syntax.</param>
        /// <returns>The found SerializedProperty, or <c>null</c> if not found.</returns>
        public static SerializedProperty FindProp(SerializedObject so, string niceName)
        {
            return so.FindProperty(niceName) ?? so.FindProperty($"<{niceName}>k__BackingField");
        }
    }
}
#endif