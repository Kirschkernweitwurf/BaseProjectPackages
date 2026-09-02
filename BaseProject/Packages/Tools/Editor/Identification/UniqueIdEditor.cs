using Base.UtilityPackage.Identification;
using UnityEditor;

namespace Base.ToolsPackage.Editor.Identification
{
    /// <summary>
    /// Custom editor for <see cref="UniqueIdScriptableObject"/> that displays the unique ID in a read-only manner.
    /// </summary>
    [CustomEditor(typeof(UniqueIdScriptableObject))]
    internal class UniqueIdEditor : UnityEditor.Editor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is not UniqueIdScriptableObject data)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Unique ID", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(data.UniqueId ?? "<No ID>", EditorStyles.textField);
        }
    }
}