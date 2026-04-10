using UnityEditor;

namespace Utility.Editor
{
    /// <summary>
    /// Provides commonly used constant values for Unity Editor scripts.
    /// </summary>
    public static class EditorConstants
    {
        /// <summary>
        /// The internal serialized property name Unity uses for the script reference field.
        /// </summary>
        /// <remarks>
        /// This property appears at the top of every custom inspector and refers to the
        /// <see cref="MonoScript"/> asset that defines the component or ScriptableObject.
        /// </remarks>
        public const string ScriptPropertyName = "m_Script";
    }
}