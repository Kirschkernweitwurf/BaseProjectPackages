using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The inspector of a theme asset. Draws the same sections the Editor UI Theme settings page does,
    /// so a theme can be edited from wherever it was opened.
    /// </summary>
    internal sealed class EditorThemeInspector : UnityEditor.Editor
    {
        private const string ActivateLabel = "Use This Theme";
        private const string ActiveMessage = "This is the theme the project draws with.";
        private const string CustomMessage = "Its colors do not match any preset.";
        private const string InactiveMessage = "Another theme is active, so editing this one changes "
            + "nothing until it is picked.";
        private const string MatchMessage = "Currently the {0} preset, unchanged.";
        private const string PageHint = "Presets and a live preview live in Project Settings under "
            + "Base Tools, Editor UI Theme.";

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            EditorTheme theme = target as EditorTheme;

            if (theme == null)
                return;

            DrawActivation(theme);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            EditorThemeGui.Draw(serializedObject);
        }

        private static void DrawActivation(EditorTheme theme)
        {
            bool isActive = EditorThemeProvider.ActiveTheme == theme;

            string state = EditorThemePresets.TryIdentify(theme, out EEditorThemePreset preset)
                ? string.Format(MatchMessage, EditorThemePresets.DisplayName(preset))
                : CustomMessage;

            string message = isActive
                ? ActiveMessage
                : InactiveMessage;

            EditorGUILayout.HelpBox($"{message} {state}", isActive
                ? MessageType.Info
                : MessageType.None);

            EditorGUILayout.HelpBox(PageHint, MessageType.None);

            if (isActive)
                return;

            if (GUILayout.Button(ActivateLabel))
                EditorThemeProvider.SetActiveTheme(theme);
        }
    }
}