using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Draws the editable body of a theme: the colors of both skins, the layout metrics and the
    /// numbers a list window is built from.
    /// </summary>
    /// <remarks>
    /// Shared by the Editor UI Theme settings page and the inspector of a theme asset, so a theme
    /// is edited the same way wherever it is opened from and neither has to be kept in step with the
    /// other when a value is added.
    /// </remarks>
    public static class EditorThemeGui
    {
        private const string DarkLabel = "Dark Skin Colors";
        private const string DarkTooltip = "Used while the editor runs the dark skin.";
        private const string LightLabel = "Light Skin Colors";
        private const string LightTooltip = "Used while the editor runs the light skin.";
        private const string MetricsLabel = "Layout";
        private const string MetricsTooltip = "Spacings, sizes and corner radii every Base window lays out by.";
        private const string ResetLabel = "Reset To Built-in Look";
        private const string ResetMessage = "Every color and size in this theme is replaced by the built-in look. "
            + "This cannot be undone from here.";
        private const string ResetNo = "Cancel";
        private const string ResetTitle = "Reset theme";
        private const string ResetYes = "Reset";
        private const string TableLabel = "List Windows";
        private const string TableTooltip = "The card, badges, ping button and toolbar of a Base list window.";

        private static readonly GUIContent DarkContent = new(DarkLabel, DarkTooltip);
        private static readonly GUIContent LightContent = new(LightLabel, LightTooltip);
        private static readonly GUIContent MetricsContent = new(MetricsLabel, MetricsTooltip);
        private static readonly GUIContent TableContent = new(TableLabel, TableTooltip);

        /// <summary>
        /// Draws every section of a theme and writes back whatever the user changed.
        /// </summary>
        /// <param name="serializedObject">The serialized theme to edit.</param>
        /// <returns>True when something changed, so the caller can react beyond the repaint.</returns>
        public static bool Draw(SerializedObject serializedObject)
        {
            if (serializedObject == null)
                return false;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            DrawSection(serializedObject, EditorTheme.DarkColorsPropertyName, DarkContent);
            DrawSection(serializedObject, EditorTheme.LightColorsPropertyName, LightContent);
            DrawSection(serializedObject, EditorTheme.MetricsPropertyName, MetricsContent);
            DrawSection(serializedObject, EditorTheme.TablePropertyName, TableContent);

            if (!EditorGUI.EndChangeCheck())
                return false;

            serializedObject.ApplyModifiedProperties();

            EditorThemeProvider.NotifyChanged();

            return true;
        }

        /// <summary>
        /// Draws the button that puts a theme back to the built-in look, behind a confirmation.
        /// </summary>
        /// <param name="theme">The theme to reset.</param>
        /// <returns>True when the theme was reset.</returns>
        public static bool DrawResetButton(EditorTheme theme)
        {
            if (theme == null)
                return false;

            if (!GUILayout.Button(ResetLabel))
                return false;

            if (!EditorUtility.DisplayDialog(ResetTitle, ResetMessage, ResetYes, ResetNo))
                return false;

            Undo.RecordObject(theme, ResetTitle);

            theme.ResetToDefaults();

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssetIfDirty(theme);

            EditorThemeProvider.NotifyChanged();

            return true;
        }

        private static void DrawSection(SerializedObject serializedObject, string propertyName, GUIContent label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                return;

            // Drawn with its children rather than field by field, so every Range and Min the data
            // classes declare keeps working and a value added there needs no change here.
            EditorGUILayout.PropertyField(property, label, true);
            EditorGUILayout.Space(EditorMetrics.TightGap);
        }
    }
}