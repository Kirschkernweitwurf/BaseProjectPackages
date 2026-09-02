using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.BaseToolsOverview;
using UnityEditor;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Exposes the Codebase Graph ignore list in the project settings, so third party code under Assets
    /// can be declared out of scope once instead of dismissed finding by finding in every project.
    /// </summary>
    internal static class CodebaseGraphSettingsProvider
    {
        private const string Help = "Scripts whose path contains one of these are left out of every "
            + "finding. Use it for code you did not write and are not going to fix. "
            + "Example: \"/CleverClicker/\"";

        private const string IgnoredHeader = "Ignored Paths";
        private const string PageLabel = "Codebase Graph";
        private const string SettingsPath = "Project/Base Tools/Codebase Graph";
        private const string Summary = "The paths the codebase scan leaves out of every finding.";

        private static readonly EditorWindowStyles Styles = new();

        private static SerializedObject _serializedObject;
        private static SerializedProperty _fragmentsProperty;

        [SettingsProvider]
        [BaseToolsPage(Summary)]
        private static SettingsProvider Create() => new(SettingsPath, SettingsScope.Project)
        {
            label = PageLabel,
            keywords = new HashSet<string>
            {
                "codebase",
                "graph",
                "dead",
                "findings",
                "ignore"
            },

            // Created lazily so the singleton is not loaded on every domain reload, only once this page
            // is actually opened.
            activateHandler = (_, _) =>
            {
                _serializedObject = new SerializedObject(CodebaseGraphSettings.instance);
                _fragmentsProperty =
                    _serializedObject.FindProperty(CodebaseGraphSettings.FragmentsPropertyName);
            },
            deactivateHandler = () =>
            {
                _serializedObject?.Dispose();
                _serializedObject = null;
                _fragmentsProperty = null;

                Styles.Dispose();
            },
            guiHandler = _ => DrawGui()
        };

        private static void DrawGui()
        {
            if (_serializedObject == null)
                return;

            Styles.EnsureBuilt();

            _serializedObject.Update();

            EditorWindowChrome.DrawIntro(Styles, Summary);

            EditorGUILayout.HelpBox(Help, MessageType.Info);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            EditorWindowChrome.DrawSectionHeader(Styles, IgnoredHeader);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_fragmentsProperty, true);

            if (!EditorGUI.EndChangeCheck())
                return;

            _serializedObject.ApplyModifiedProperties();
            CodebaseGraphSettings.instance.Persist();
        }
    }
}