using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// Which <see cref="EditorTheme"/> the project draws with, kept in
    /// <c>ProjectSettings/BaseEditorTheme.asset</c> so the choice travels with the project rather
    /// than with the machine.
    /// </summary>
    /// <remarks>
    /// The theme is referenced by asset GUID rather than by object reference, because a settings
    /// file outside the Assets folder cannot hold one. A GUID also survives the asset being renamed
    /// or moved.
    /// </remarks>
    [FilePath("ProjectSettings/BaseEditorTheme.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class EditorThemeSettings : ScriptableSingleton<EditorThemeSettings>
    {
        [SerializeField] private string themeGuid;

        /// <summary>The GUID of the assigned theme asset, or empty while the built-in look is used.</summary>
        public string ThemeGuid => themeGuid ?? string.Empty;

        /// <summary>
        /// Points the project at a theme asset, or at the built-in look when handed nothing.
        /// </summary>
        /// <param name="guid">The GUID of the theme asset, or null or empty for the built-in look.</param>
        public void SetThemeGuid(string guid)
        {
            string resolved = guid ?? string.Empty;

            if (resolved == ThemeGuid)
                return;

            themeGuid = resolved;

            Save(true);
        }
    }
}