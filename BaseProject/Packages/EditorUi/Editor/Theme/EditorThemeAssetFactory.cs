using System.IO;
using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Creates a theme asset, seeded with the built-in look, at a place the user picks.
    /// </summary>
    /// <remarks>
    /// A save panel rather than an entry under Assets > Create, because a theme is created once and
    /// then edited from the settings page, and because a menu entry in a Base package belongs in the
    /// menu manager, which this package sits below and cannot reach.
    /// </remarks>
    public static class EditorThemeAssetFactory
    {
        private const string DefaultFileName = "EditorTheme";
        private const string DefaultFolder = "Assets";
        private const string Extension = "asset";
        private const string SaveTitle = "Create Editor UI Theme";

        /// <summary>
        /// Asks where to put a new theme, creates it, selects it and makes it the active one.
        /// </summary>
        /// <returns>The created theme, or null when the user cancelled the save panel.</returns>
        public static EditorTheme CreateAndActivate()
        {
            string path = EditorUtility.SaveFilePanelInProject(SaveTitle, DefaultFileName, Extension,
                string.Empty, DefaultFolder);

            if (string.IsNullOrEmpty(path))
                return null;

            EditorTheme theme = ScriptableObject.CreateInstance<EditorTheme>();

            theme.ResetToDefaults();
            theme.name = Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(theme, path);
            AssetDatabase.SaveAssets();

            EditorThemeProvider.SetActiveTheme(theme);

            Selection.activeObject = theme;

            return theme;
        }
    }
}