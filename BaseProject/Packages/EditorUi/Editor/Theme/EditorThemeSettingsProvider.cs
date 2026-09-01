using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// The Editor UI Theme page in the project settings: what the project draws with now, five looks
    /// to start from, a live preview of either editor theme, and the values behind it all.
    /// </summary>
    /// <remarks>
    /// Ordered the way the job is actually done: see what is active, pick a starting point, look at
    /// it, then edit. The values come last and stay folded away, because most visits end at the
    /// preset row.
    /// </remarks>
    internal static class EditorThemeSettingsProvider
    {
        private const string CreateLabel = "Create Theme Asset";
        private const string CustomStatus = "Editing \"{0}\". Its colors no longer match any preset.";
        private const string DarkLabel = "Dark";
        private const string DefaultsStatus = "No theme assigned, so every Base window draws with the "
            + "built-in Slate look. Pick a preset below to make it yours.";
        private const string LightLabel = "Light";
        private const string MatchStatus = "Editing \"{0}\", currently the {1} preset unchanged.";
        private const float ModeButtonWidth = 56f;
        private const string ModeTooltip = "Whether the preview and the swatches are drawn in dark "
            + "mode or light mode. Unity's own Editor Theme is left alone.";
        private const string PageLabel = "Editor UI Theme";
        private const float PresetButtonHeight = 46f;
        private const float PresetButtonMinWidth = 84f;
        private const string PresetMessage = "Replaces every color and size in \"{0}\" with the {1} preset. "
            + "This cannot be undone from here.";
        private const string PresetNo = "Cancel";
        private const string PresetsHeader = "Start From";
        private const string PresetTitle = "Apply preset";
        private const string PresetYes = "Apply";
        private const string PreviewHeader = "Preview";
        private const string PreviewLiveNote = "This is what Unity's Editor Theme is set to.";
        private const string PreviewOtherNote = "Unity's Editor Theme is set to the other one, so only "
            + "this panel changes.";
        private const string SettingsPath = "Project/Base Tools/Editor UI Theme";
        private const float SwatchGap = 3f;
        private const float SwatchHeight = 10f;
        private const float SwatchInset = 8f;
        private const string ThemeLabel = "Theme";
        private const string ThemeTooltip = "The theme asset the project draws with. Leave it empty for "
            + "the built-in Slate look.";
        private const string UseDefaultsLabel = "Use Built-in Look";
        private const float UseDefaultsWidth = 130f;

        private static readonly GUIContent DarkContent = new(DarkLabel, ModeTooltip);
        private static readonly GUIContent LightContent = new(LightLabel, ModeTooltip);
        private static readonly GUIContent ThemeContent = new(ThemeLabel, ThemeTooltip);
        private static readonly EditorTableStyles Styles = new();

        private static SerializedObject _serializedObject;
        private static EditorTheme _boundTheme;
        private static Vector2 _scroll;

        // Null until the user picks a side, so the page opens on whichever editor theme they are running.
        private static bool? _previewDarkMode;

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
                "preset",
                "contrast",
                "base"
            },

            activateHandler = (_, _) => _scroll = Vector2.zero,
            deactivateHandler = Release,
            guiHandler = _ => DrawGui()
        };

        private static void Release()
        {
            _serializedObject?.Dispose();
            _serializedObject = null;
            _boundTheme = null;

            // Only ever set and cleared inside one draw, but clearing it here too means a page torn
            // down mid pass cannot strand the editor on the previewed editor theme.
            EditorThemeProvider.EndDarkModeOverride();

            Styles.Dispose();
        }

        private static void DrawGui()
        {
            Styles.EnsureBuilt();

            EditorTheme theme = EditorThemeProvider.ActiveTheme;

            Rebind(theme);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawStatus(theme);
            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawThemePicker(theme);
            EditorGUILayout.Space(EditorMetrics.SectionGap);

            if (theme != null)
            {
                DrawPresets(theme);
                EditorGUILayout.Space(EditorMetrics.SectionGap);
            }

            DrawPreview();

            if (theme != null)
            {
                EditorGUILayout.Space(EditorMetrics.SectionGap);
                EditorThemeGui.Draw(_serializedObject);
            }

            EditorGUILayout.EndScrollView();
        }

        // One line saying exactly where the project stands, so the rest of the page has a subject.
        private static void DrawStatus(EditorTheme theme)
        {
            if (theme == null)
            {
                EditorGUILayout.HelpBox(DefaultsStatus, MessageType.Info);
                return;
            }

            string message = EditorThemePresets.TryIdentify(theme, out EEditorThemePreset preset)
                ? string.Format(MatchStatus, theme.name, EditorThemePresets.DisplayName(preset))
                : string.Format(CustomStatus, theme.name);

            EditorGUILayout.HelpBox(message, MessageType.None);
        }

        private static void DrawThemePicker(EditorTheme theme)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            EditorTheme picked = EditorGUILayout.ObjectField(ThemeContent, theme, typeof(EditorTheme),
                false) as EditorTheme;

            if (EditorGUI.EndChangeCheck())
                EditorThemeProvider.SetActiveTheme(picked);

            if (theme == null)
            {
                if (GUILayout.Button(CreateLabel, GUILayout.Width(UseDefaultsWidth)))
                    EditorThemeAssetFactory.CreateAndActivate();
            }
            else if (GUILayout.Button(UseDefaultsLabel, GUILayout.Width(UseDefaultsWidth)))
            {
                EditorThemeProvider.SetActiveTheme(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawPresets(EditorTheme theme)
        {
            GUILayout.Label(PresetsHeader, EditorStyles.boldLabel);

            bool hasMatch = EditorThemePresets.TryIdentify(theme, out EEditorThemePreset current);
            bool isDark = IsPreviewDark();

            EditorGUILayout.BeginHorizontal();

            foreach (EEditorThemePreset preset in EditorThemePresets.CreateOrder())
                DrawPresetButton(theme, preset, hasMatch && preset == current, isDark);

            EditorGUILayout.EndHorizontal();
        }

        // The button keeps the same chrome as every other button on the page and shows the preset's
        // own colors as a swatch strip instead. Painting the button itself would leave five different
        // looks arguing with each other on the one page whose job is judging a look.
        private static void DrawPresetButton(EditorTheme theme, EEditorThemePreset preset, bool isCurrent,
            bool isDark)
        {
            GUIContent content = new(EditorThemePresets.DisplayName(preset),
                EditorThemePresets.Description(preset));

            // Reserved without the content, because a rectangle measured from the label gives every
            // button a different minimum and the row then shares the leftover space in proportion to
            // it. Five identical minimums are what makes the five buttons come out the same width.
            Rect area = GUILayoutUtility.GetRect(0f, PresetButtonHeight,
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(PresetButtonMinWidth));

            bool isPressed = GUI.Button(area, GUIContent.none);

            GUI.Label(new Rect(area.x, area.y + EditorMetrics.TightGap, area.width,
                EditorGUIUtility.singleLineHeight), content, Styles.Badge);

            DrawSwatches(SwatchArea(area), preset, isDark);

            if (isCurrent)
                DrawSelectionOutline(area);

            if (!isPressed)
                return;

            Apply(theme, preset);
        }

        private static Rect SwatchArea(Rect area) => new(area.x + SwatchInset,
            area.yMax - SwatchHeight - EditorMetrics.TightGap * 2f,
            area.width - SwatchInset * 2f, SwatchHeight);

        private static void DrawSwatches(Rect area, EEditorThemePreset preset, bool isDark)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Color[] swatches = EditorThemePresets.CreateSwatches(preset, isDark);
            float width = (area.width - SwatchGap * (swatches.Length - 1)) / swatches.Length;

            for (int i = 0; i < swatches.Length; i++)
            {
                Rect chip = new(area.x + i * (width + SwatchGap), area.y, width, area.height);

                EditorGUI.DrawRect(chip, swatches[i]);
            }
        }

        // Drawn as an outline rather than a fill, so the swatches underneath stay readable.
        private static void DrawSelectionOutline(Rect area)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            float thickness = EditorMetrics.SeparatorThickness * 2f;
            Color color = EditorPalette.Accent;

            EditorGUI.DrawRect(new Rect(area.x, area.y, area.width, thickness), color);
            EditorGUI.DrawRect(new Rect(area.x, area.yMax - thickness, area.width, thickness), color);
            EditorGUI.DrawRect(new Rect(area.x, area.y, thickness, area.height), color);
            EditorGUI.DrawRect(new Rect(area.xMax - thickness, area.y, thickness, area.height), color);
        }

        private static void Apply(EditorTheme theme, EEditorThemePreset preset)
        {
            string message = string.Format(PresetMessage, theme.name,
                EditorThemePresets.DisplayName(preset));

            if (!EditorUtility.DisplayDialog(PresetTitle, message, PresetYes, PresetNo))
                return;

            Undo.RecordObject(theme, PresetTitle);

            EditorThemePresets.Apply(theme, preset);

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssetIfDirty(theme);

            EditorThemeProvider.NotifyChanged();

            _serializedObject.Update();
        }

        private static bool IsPreviewDark() => _previewDarkMode ?? EditorGUIUtility.isProSkin;

        private static void DrawPreview()
        {
            bool isDark = IsPreviewDark();

            DrawPreviewToolbar(isDark);

            // Built inside the override as well as drawn inside it, because a style pins its text
            // colors and generates its textures when it is built. Cleared in a finally so a throw in
            // the middle cannot leave the rest of the editor on the wrong editor theme.
            EditorThemeProvider.BeginDarkModeOverride(isDark);

            try
            {
                Styles.EnsureBuilt();

                Rect area = GUILayoutUtility.GetRect(0f, EditorThemePreview.MeasureHeight(),
                    GUILayout.ExpandWidth(true));

                EditorThemePreview.Draw(area, Styles);
            }
            finally
            {
                EditorThemeProvider.EndDarkModeOverride();
            }
        }

        private static void DrawPreviewToolbar(bool isDark)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(PreviewHeader, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            DrawModeButton(DarkContent, true, isDark);
            DrawModeButton(LightContent, false, isDark);

            EditorGUILayout.EndHorizontal();

            // Drawn either way rather than only when it applies, so switching sides does not move the
            // preview up and down by a line.
            GUILayout.Label(isDark == EditorGUIUtility.isProSkin
                ? PreviewLiveNote
                : PreviewOtherNote, EditorStyles.miniLabel);
        }

        private static void DrawModeButton(GUIContent content, bool isDarkButton, bool isDark)
        {
            bool isSelected = isDarkButton == isDark;

            if (GUILayout.Toggle(isSelected, content, EditorStyles.miniButton,
                    GUILayout.Width(ModeButtonWidth))
                == isSelected)
                return;

            _previewDarkMode = isDarkButton;
        }

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

        // The target is checked as well as the theme, because a deleted asset reads as null through
        // Unity's operator and would otherwise compare equal to having no theme at all, leaving the
        // page bound to an object that throws the moment it is read.
        private static bool IsBindingCurrent(EditorTheme theme)
        {
            if (theme == null)
                return _serializedObject == null;

            return _boundTheme == theme
                && _serializedObject != null
                && _serializedObject.targetObject != null;
        }
    }
}