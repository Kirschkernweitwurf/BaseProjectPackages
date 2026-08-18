using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.OverviewGui
{
    /// <summary>
    /// Shared styles and layout blocks for the project health overview windows, so that the
    /// unused assets and unused scripts windows stay visually identical without copied code.
    /// <para>
    /// The shared editor look lives in <see cref="EditorPalette"/> and <see cref="EditorMetrics"/>;
    /// what stays here are the two accent colors that stand for the two kinds of finding, and the
    /// section header that is this window family's own signature.
    /// </para>
    /// </summary>
    public static class OverviewGui
    {
        public const float RowHeight = EditorMetrics.RowHeight;

        private const float ActiveHandleAlpha = 0.35f;
        private const float BadgeTrailing = 36f;
        private const float BadgeWidth = 30f;
        private const float BandWidth = 3f;
        private const string ByteSuffix = " B";
        private const float FoldoutInset = 10f;
        private const float FoldoutTrailing = 54f;
        private const float FoldoutVerticalInset = 3f;
        private const float HandleHeight = 6f;
        private const float HandleLineHeight = 2f;
        private const float HandleLineWidth = 28f;
        private const int HeaderFontSize = 12;
        private const float HeaderTint = 0.16f;
        private const float IdleHandleAlpha = 0.14f;
        private const string KiloByteSuffix = " KB";
        private const float KiloBytes = 1024f;
        private const string MegaByteSuffix = " MB";
        private const float SectionHeight = 22f;
        private const float SectionInset = 2f;
        private const float SectionTop = 4f;
        private const string SizeFormat = "0.0";
        private const float SuccessGap = 8f;
        private const string SuccessIcon = "TestPassed";
        private const float SuccessIconSize = 48f;
        private const int SuccessTitleFontSize = 15;

        public static GUIStyle HeaderStyle { get; private set; }

        public static GUIStyle GroupStyle { get; private set; }

        public static GUIStyle PathStyle { get; private set; }

        public static GUIStyle WarningBadgeStyle { get; private set; }

        public static GUIStyle NeutralBadgeStyle { get; private set; }

        // Calm blue reads as stored, warning yellow matches the Unity console warning icon.
        private static readonly Color NeutralAccent = new(0.33f, 0.52f, 0.74f);
        private static readonly Color WarningAccent = new(0.96f, 0.78f, 0.12f);
        private static readonly Color WarningBadgeText = new(0.15f, 0.13f, 0.05f);

        // Deliberately stronger than the shared hairline: it closes off a whole section.
        private static readonly Color SectionUnderline = new(0f, 0f, 0f, 0.25f);

        private static readonly EditorTextureCache Textures = new();

        private static GUIStyle _sectionFoldoutStyle;
        private static GUIStyle _successTitleStyle;
        private static GUIStyle _successSubtitleStyle;
        private static Texture _successTexture;
        private static bool _ready;
        private static bool _builtForProSkin;

        /// <summary>
        /// Builds the shared styles once per domain, and again after a skin change.
        /// Call this at the top of OnGUI.
        /// </summary>
        public static void EnsureStyles()
        {
            if (_ready && _builtForProSkin == EditorGUIUtility.isProSkin)
                return;

            // The generated badge backgrounds are hidden and not saved, so the previous ones would
            // leak on every rebuild if they were not released first.
            Textures.Release();

            HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = HeaderFontSize
            };

            GroupStyle = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(2, 2, 2, 0),
                padding = new RectOffset(6, 6, 3, 3)
            };

            PathStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            _sectionFoldoutStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            WarningBadgeStyle = MakeBadgeStyle(WarningAccent, WarningBadgeText);
            NeutralBadgeStyle = MakeBadgeStyle(NeutralAccent, Color.white);

            _successTitleStyle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = SuccessTitleFontSize
            }, EditorPalette.Success);

            _successSubtitleStyle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            }, EditorStyleUtility.MutedTextColor());

            _successTexture = EditorGUIUtility.IconContent(SuccessIcon).image;

            _ready = true;
            _builtForProSkin = EditorGUIUtility.isProSkin;
        }

        /// <summary>Returns the badge style matching the accent.</summary>
        /// <param name="accent">The accent this kind of finding is reported with.</param>
        /// <returns>The badge style the count is drawn with.</returns>
        public static GUIStyle BadgeStyle(EOverviewAccent accent) => accent == EOverviewAccent.Neutral
            ? NeutralBadgeStyle
            : WarningBadgeStyle;

        /// <summary>
        /// Draws a tinted, color coded section header with a count badge and returns the new expanded state.
        /// </summary>
        /// <param name="expanded">Whether the section is currently expanded.</param>
        /// <param name="label">The label of the section.</param>
        /// <param name="count">The number of findings in the section.</param>
        /// <param name="accent">The accent the section is drawn with.</param>
        /// <returns>The expanded state after the user interacted with the foldout.</returns>
        public static bool DrawSectionHeader(bool expanded, string label, int count, EOverviewAccent accent)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, SectionHeight, GUILayout.ExpandWidth(true));
            rect = new Rect(rect.x + SectionInset, rect.y + SectionTop, rect.width - SectionInset * 2f,
                rect.height);

            Color accentColor = AccentColor(accent);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(accentColor.r, accentColor.g, accentColor.b, HeaderTint));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, BandWidth, rect.height), accentColor);

                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - EditorMetrics.SeparatorThickness, rect.width,
                    EditorMetrics.SeparatorThickness), SectionUnderline);
            }

            Rect foldoutRect = new(rect.x + FoldoutInset, rect.y + FoldoutVerticalInset,
                rect.width - FoldoutTrailing, rect.height - FoldoutVerticalInset * 2f);

            Rect badgeRect = new(rect.xMax - BadgeTrailing,
                rect.y + (rect.height - EditorMetrics.BadgeHeight) * 0.5f, BadgeWidth, EditorMetrics.BadgeHeight);

            bool result = EditorGUI.Foldout(foldoutRect, expanded, label, true, _sectionFoldoutStyle);
            GUI.Label(badgeRect, count.ToString(), BadgeStyle(accent));

            return result;
        }

        /// <summary>
        /// Draws a drag strip that resizes the block above it and returns the new height.
        /// The height is clamped to the given range and stored under the EditorPrefs key when the drag ends.
        /// </summary>
        /// <param name="height">The current height of the block above the handle.</param>
        /// <param name="minHeight">The smallest height the block may take.</param>
        /// <param name="maxHeight">The largest height the block may take.</param>
        /// <param name="prefsKey">The EditorPrefs key the height is stored under.</param>
        /// <returns>The height after this event was handled.</returns>
        public static float DrawResizeHandle(float height, float minHeight, float maxHeight, string prefsKey)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, HandleHeight, GUILayout.ExpandWidth(true));
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType type = Event.current.GetTypeForControl(controlId);
            bool active = GUIUtility.hotControl == controlId;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            if (type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.center.x - HandleLineWidth * 0.5f,
                        rect.center.y - HandleLineHeight * 0.5f, HandleLineWidth, HandleLineHeight),
                    EditorPalette.Tint(active
                        ? ActiveHandleAlpha
                        : IdleHandleAlpha));

            if (type == EventType.MouseDown
                && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = controlId;
                Event.current.Use();
                return height;
            }

            if (!active)
                return height;

            if (type == EventType.MouseDrag)
            {
                Event.current.Use();
                return Mathf.Clamp(height + Event.current.delta.y, minHeight, Mathf.Max(minHeight, maxHeight));
            }

            if (type != EventType.MouseUp)
                return height;

            GUIUtility.hotControl = 0;
            EditorPrefs.SetFloat(prefsKey, height);
            Event.current.Use();

            return height;
        }

        /// <summary>Fills a row rect with the striping and hover tint used by the overview windows.</summary>
        /// <param name="rect">The full row rectangle.</param>
        /// <param name="hovered">Whether the mouse sits on the row.</param>
        /// <param name="even">Whether this is an even row, which is the one that gets striped.</param>
        public static void DrawRowBackground(Rect rect, bool hovered, bool even)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (hovered)
            {
                EditorGUI.DrawRect(rect, EditorPalette.Hover);
                return;
            }

            if (even)
                EditorGUI.DrawRect(rect, EditorPalette.Stripe);
        }

        /// <summary>Draws an inline hint box below the list.</summary>
        /// <param name="message">The hint to show.</param>
        public static void DrawHint(string message)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(message, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        /// <summary>Draws the empty state shown when a scan found nothing to report.</summary>
        /// <param name="title">The headline of the empty state.</param>
        /// <param name="subtitle">The explanation shown under the headline.</param>
        public static void DrawSuccess(string title, string subtitle)
        {
            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(new GUIContent(_successTexture),
                    GUILayout.Width(SuccessIconSize),
                    GUILayout.Height(SuccessIconSize));

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(SuccessGap);

            GUILayout.Label(title, _successTitleStyle);
            GUILayout.Label(subtitle, _successSubtitleStyle);

            GUILayout.FlexibleSpace();
        }

        /// <summary>Selects and pings the asset at the given project relative path.</summary>
        /// <param name="assetPath">The project relative path of the asset to reveal.</param>
        public static void Navigate(string assetPath)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        /// <summary>Picks the singular or plural word for the given amount.</summary>
        /// <param name="amount">The amount the word describes.</param>
        /// <param name="singular">The word used for exactly one.</param>
        /// <param name="plural">The word used for every other amount.</param>
        /// <returns>The word matching the amount.</returns>
        public static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        /// <summary>Formats a byte count as a short human readable size.</summary>
        /// <param name="bytes">The size in bytes.</param>
        /// <returns>The size as a short string with its unit.</returns>
        public static string FormatSize(long bytes)
        {
            if (bytes >= KiloBytes * KiloBytes)
                return (bytes / (KiloBytes * KiloBytes)).ToString(SizeFormat) + MegaByteSuffix;

            if (bytes >= KiloBytes)
                return (bytes / KiloBytes).ToString(SizeFormat) + KiloByteSuffix;

            return bytes + ByteSuffix;
        }

        private static Color AccentColor(EOverviewAccent accent) => accent == EOverviewAccent.Neutral
            ? NeutralAccent
            : WarningAccent;

        private static GUIStyle MakeBadgeStyle(Color fill, Color textColor) => new(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = textColor,
                background = Textures.Solid(fill)
            }
        };
    }
}