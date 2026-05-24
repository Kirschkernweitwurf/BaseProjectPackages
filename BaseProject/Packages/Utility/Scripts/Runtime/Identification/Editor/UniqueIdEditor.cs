using UnityEditor;

namespace Base.UtilityPackage.Identification.Editor
{
    /// <summary>
    /// Custom editor for <see cref="UniqueIdScriptableObject"/> that displays the unique ID in a read-only manner.
    /// </summary>
    [CustomEditor(typeof(UniqueIdScriptableObject))]
    public class UniqueIdEditor : UnityEditor.Editor
    {
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