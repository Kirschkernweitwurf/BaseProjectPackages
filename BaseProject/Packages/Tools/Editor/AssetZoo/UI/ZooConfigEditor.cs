using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using Base.ToolsPackage.Editor.AssetZoo.Generation;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Custom inspector for <see cref="ZooConfig"/> that adds buttons for generating categories and
    /// opening the zoo window, and reports what the last generation run did.
    /// </summary>
    [CustomEditor(typeof(ZooConfig))]
    internal class ZooConfigEditor : UnityEditor.Editor
    {
        private const string ActionsHeader = "Actions";
        private const float ButtonHeight = 24f;
        private const string GenerateLabel = "Auto Generate Categories";
        private const string OpenWindowLabel = "Open Zoo Window";

        // Unity's own name for the hidden script reference, which has no member to point nameof at.
        private const string ScriptField = "m_Script";

        private readonly EditorWindowStyles _styles = new();

        private ZooGenerationResult _lastResult;
        private bool _hasResult;

#region Unity Callbacks
        private void OnDisable() => _styles.Dispose();
#endregion

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            _styles.EnsureBuilt();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ScriptField);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(EditorMetrics.SectionGap);
            EditorWindowChrome.DrawSectionHeader(_styles, ActionsHeader);

            if (EditorWindowChrome.PrimaryButton(_styles, GenerateLabel, GUILayout.Height(ButtonHeight)))
                Generate();

            EditorGUILayout.Space(EditorMetrics.TightGap);

            if (EditorWindowChrome.SecondaryButton(_styles, OpenWindowLabel, GUILayout.Height(ButtonHeight)))
                ZooEditorWindow.Open((ZooConfig)target);

            if (!_hasResult)
                return;

            EditorGUILayout.Space(EditorMetrics.ItemGap);
            ZooResultView.Draw(_styles, _lastResult);
        }

        private void Generate()
        {
            _lastResult = ZooAutoGenerator.Generate((ZooConfig)target);
            _hasResult = true;

            serializedObject.Update();
        }
    }
}