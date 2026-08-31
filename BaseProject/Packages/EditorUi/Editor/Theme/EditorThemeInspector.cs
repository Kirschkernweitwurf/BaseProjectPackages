using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The inspector of a theme asset. Draws the same sections the Editor UI Theme settings page
    /// does, so a theme can be edited from wherever it was opened.
    /// </summary>
    [CustomEditor(typeof(EditorTheme))]
    internal sealed class EditorThemeInspector : UnityEditor.Editor
    {
        private const string ActivateLabel = "Use This Theme";
        private const string ActiveMessage = "This is the theme the project draws with.";
        private const string InactiveMessage = "Another theme is active, so editing this one changes nothing "
            + "until it is picked.";

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorTheme theme = target as EditorTheme;

            if (theme == null)
                return;

            DrawActivation(theme);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            EditorThemeGui.Draw(serializedObject);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            EditorThemeGui.DrawResetButton(theme);
        }

        private static void DrawActivation(EditorTheme theme)
        {
            bool isActive = EditorThemeProvider.ActiveTheme == theme;

            string message = isActive
                ? ActiveMessage
                : InactiveMessage;

            MessageType type = isActive
                ? MessageType.Info
                : MessageType.None;

            EditorGUILayout.HelpBox(message, type);

            if (isActive)
                return;

            if (GUILayout.Button(ActivateLabel))
                EditorThemeProvider.SetActiveTheme(theme);
        }
    }
}