using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The Editor UI Theme page in the project settings: pick the theme the project draws with,
    /// create one, edit every color and size in it, and watch a miniature list window change as you
    /// go.
    /// </summary>
    /// <remarks>
    /// Registered under the Base Tools root, so the overview page there lists it along with the rest
    /// without anything having to be registered.
    /// </remarks>
    internal static class EditorThemeSettingsProvider
    {
        private const string CreateLabel = "Create Theme Asset";
        private const string DefaultsMessage = "No theme is assigned, so every Base window draws with the "
            + "built-in look. Create one to change any of it.";
        private const string Intro = "One theme drives the look of every Base editor window: the colors, the "
            + "corner radii, the row heights and the gaps. Assign one here and the change reaches every open "
            + "window at once.";
        private const string PageLabel = "Editor UI Theme";
        private const string DarkLabel = "Dark";
        private const string LightLabel = "Light";
        private const string LiveNote = "This is the skin your editor is running.";
        private const string OtherNote = "Your editor is running the other skin, so only this panel "
            + "changes.";
        private const string PreviewHeader = "Preview";
        private const float SkinButtonWidth = 60f;
        private const string SkinTooltip = "Which skin the preview is drawn in. The editor itself is "
            + "left alone, so the colors of the skin you are not running can be judged without "
            + "switching to it.";
        private const string SettingsPath = "Project/Base Tools/Editor UI Theme";
        private const string ThemeLabel = "Theme";
        private const string ThemeTooltip = "The theme asset the project draws with. Leave it empty for the "
            + "built-in look.";
        private const string UseDefaultsLabel = "Use Built-in Look";

        private static readonly GUIContent ThemeContent = new(ThemeLabel, ThemeTooltip);
        private static readonly GUIContent DarkContent = new(DarkLabel, SkinTooltip);
        private static readonly GUIContent LightContent = new(LightLabel, SkinTooltip);
        private static readonly EditorTableStyles Styles = new();

        private static SerializedObject _serializedObject;
        private static EditorTheme _boundTheme;
        private static Vector2 _scroll;

        // Null until the user picks one, so the preview opens on whichever skin they are running
        // rather than on a side that was chosen for them.
        private static bool? _previewDarkSkin;

        [SettingsProvider]
        private static SettingsProvider Create() => new(SettingsPath, SettingsScope.Project)
        {
            label = PageLabel,
            keywords = new HashSet<string>
            {
                "theme",
                "editor",
                "ui",
                "color",
                "style",
                "look",
                "base"
            },

            // Nothing is bound here: the page rebinds on every draw when the active theme moved, and
            // that same path is what has to work after a reload with the page already open.
            activateHandler = (_, _) => _scroll = Vector2.zero,
            deactivateHandler = Release,
            guiHandler = _ => DrawGui()
        };

        private static void Release()
        {
            _serializedObject?.Dispose();
            _serializedObject = null;
            _boundTheme = null;

            // The override is only ever set and cleared inside one draw, but clearing it here too
            // means a page torn down mid pass cannot strand it.
            EditorThemeProvider.EndSkinOverride();

            Styles.Dispose();
        }

        private static void DrawGui()
        {
            Styles.EnsureBuilt();

            EditorTheme theme = EditorThemeProvider.ActiveTheme;

            Rebind(theme);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.HelpBox(Intro, MessageType.Info);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawThemePicker(theme);

            EditorGUILayout.Space(EditorMetrics.SectionGap);

            DrawPreview();

            EditorGUILayout.Space(EditorMetrics.TightGap);

            if (theme != null)
                DrawBody(theme);

            EditorGUILayout.EndScrollView();
        }

        // The bound object is compared against the active theme rather than refreshed on a callback,
        // because the theme can also change from the asset's own inspector or from another window.
        private static void Rebind(EditorTheme theme)
        {
            if (IsBindingCurrent(theme))
                return;

            _serializedObject?.Dispose();

            _boundTheme = theme;
            _serializedObject = theme == null
                ? null
                : new SerializedObject(theme);
        }

        // The target is checked as well as the theme, because a deleted asset reads as null
        // through Unity's operator and would otherwise compare equal to having no theme at all,
        // leaving the page bound to an object that throws the moment it is read.
        private static bool IsBindingCurrent(EditorTheme theme)
        {
            if (theme == null)
                return _serializedObject == null;

            return _boundTheme == theme
                && _serializedObject != null
                && _serializedObject.targetObject != null;
        }

        private static void DrawThemePicker(EditorTheme theme)
        {
            EditorGUI.BeginChangeCheck();

            EditorTheme picked = EditorGUILayout.ObjectField(ThemeContent, theme, typeof(EditorTheme),
                false) as EditorTheme;

            if (EditorGUI.EndChangeCheck())
                EditorThemeProvider.SetActiveTheme(picked);

            EditorGUILayout.Space(EditorMetrics.TightGap);

            if (theme == null)
            {
                EditorGUILayout.HelpBox(DefaultsMessage, MessageType.None);

                if (GUILayout.Button(CreateLabel))
                    EditorThemeAssetFactory.CreateAndActivate();

                return;
            }

            if (GUILayout.Button(UseDefaultsLabel))
                EditorThemeProvider.SetActiveTheme(null);
        }

        private static void DrawPreview()
        {
            bool isDark = _previewDarkSkin ?? EditorGUIUtility.isProSkin;

            DrawPreviewToolbar(isDark);

            // The styles are built inside the override as well as drawn inside it, because a style
            // pins its text colors and generates its textures when it is built. Cleared in a finally
            // so a throw in the middle cannot leave the rest of the editor on the wrong skin.
            EditorThemeProvider.BeginSkinOverride(isDark);

            try
            {
                Styles.EnsureBuilt();

                Rect area = GUILayoutUtility.GetRect(0f, EditorThemePreview.MeasureHeight(),
                    GUILayout.ExpandWidth(true));

                EditorThemePreview.Draw(area, Styles);
            }
            finally
            {
                EditorThemeProvider.EndSkinOverride();
            }
        }

        private static void DrawPreviewToolbar(bool isDark)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(PreviewHeader, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            DrawSkinButton(DarkContent, true, isDark);
            DrawSkinButton(LightContent, false, isDark);

            EditorGUILayout.EndHorizontal();

            // Drawn either way rather than only when it applies, so switching sides does not move
            // the preview up and down by a line.
            GUILayout.Label(isDark == EditorGUIUtility.isProSkin
                ? LiveNote
                : OtherNote, EditorStyles.miniLabel);
        }

        // A pair of toggles rather than a popup, so which side is being looked at is readable
        // without opening anything.
        private static void DrawSkinButton(GUIContent content, bool isDarkButton, bool isDark)
        {
            bool isSelected = isDarkButton == isDark;

            if (GUILayout.Toggle(isSelected, content, EditorStyles.miniButton,
                    GUILayout.Width(SkinButtonWidth)) == isSelected)
                return;

            _previewDarkSkin = isDarkButton;
        }

        private static void DrawBody(EditorTheme theme)
        {
            EditorGUILayout.Space(EditorMetrics.SectionGap);

            EditorThemeGui.Draw(_serializedObject);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (EditorThemeGui.DrawResetButton(theme))
                _serializedObject.Update();
        }
    }
}