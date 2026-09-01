using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Draws the editable body of a theme: the colors of both editor themes, the layout metrics and the
    /// numbers a list window is built from.
    /// </summary>
    /// <remarks>
    /// Shared by the Editor UI Theme settings page and the inspector of a theme asset, so a theme is
    /// edited the same way wherever it is opened from and neither has to be kept in step with the
    /// other when a value is added.
    /// <para>
    /// Every section starts folded. Picking a preset is what most visits are for, and four open
    /// blocks of sixty odd fields buries everything above them.
    /// </para>
    /// </remarks>
    public static class EditorThemeGui
    {
        private const string DarkLabel = "Dark Editor Colors";
        private const string DarkTooltip = "Used while Unity's Editor Theme is set to Dark.";
        private const string FoldoutKeyPrefix = "Base.EditorUi.Theme.Section.";
        private const string LightLabel = "Light Editor Colors";
        private const string LightTooltip = "Used while Unity's Editor Theme is set to Light.";
        private const string MetricsLabel = "Layout";
        private const string MetricsTooltip = "Spacings, sizes and corner radii every Base window lays out by.";
        private const string SectionsHeader = "Values";
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

            GUILayout.Label(SectionsHeader, EditorStyles.boldLabel);

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

        // The open state is kept per section rather than on the property, because the settings page and
        // the asset inspector draw the same theme and should agree on what is folded away.
        private static void DrawSection(SerializedObject serializedObject, string propertyName,
            GUIContent label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
                return;

            string key = FoldoutKeyPrefix + propertyName;
            bool isOpen = EditorPrefs.GetBool(key, false);

            EditorGUI.BeginChangeCheck();

            bool wanted = EditorGUILayout.Foldout(isOpen, label, true);

            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(key, wanted);

            if (!wanted)
                return;

            EditorGUI.indentLevel++;

            // Drawn child by child rather than as one property field, so the section's own foldout is
            // the only one the user has to open.
            SerializedProperty child = property.Copy();
            SerializedProperty end = property.GetEndProperty();

            if (child.NextVisible(true))
                while (!SerializedProperty.EqualContents(child, end))
                {
                    EditorGUILayout.PropertyField(child, true);

                    if (!child.NextVisible(false))
                        break;
                }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }
    }
}