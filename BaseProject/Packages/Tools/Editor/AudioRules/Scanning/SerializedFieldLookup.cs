using UnityEditor;

namespace Base.ToolsPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Finds a serialized field by the name it has in code. A property written as
    /// <c>[field: SerializeField] public T Name { get; }</c> is stored under the compiler generated
    /// backing field name, so both spellings are tried before the lookup gives up.
    /// </summary>
    internal static class SerializedFieldLookup
    {
        private const string BackingFieldPrefix = "<";
        private const string BackingFieldSuffix = ">k__BackingField";

        /// <summary>Finds the property behind a field or auto property name.</summary>
        /// <param name="source">The serialized object to search.</param>
        /// <param name="fieldName">The name the field has in code.</param>
        /// <returns>The property, or null when the object has no such field.</returns>
        internal static SerializedProperty Find(SerializedObject source, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            SerializedProperty direct = source.FindProperty(fieldName);

            if (direct != null)
                return direct;

            return source.FindProperty(BackingFieldPrefix + fieldName + BackingFieldSuffix);
        }
    }
}