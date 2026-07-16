using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.OverviewGui
{
    /// <summary>
    /// Shared styles and layout blocks for the project health overview windows, so that the
    /// unused assets and unused scripts windows stay visually identical without copied code.
    /// </summary>
    public static class OverviewGui
    {
        public const float RowHeight = 22f;

        private const float BadgeHeight = 16f;
        private const float BadgeWidth = 30f;
        private const float HandleHeight = 6f;
        private const float SectionHeight = 22f;
        private const float SuccessGap = 8f;
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
        private static readonly Color RowEvenColor = new(0f, 0f, 0f, 0.06f);
        private static readonly Color RowHoverColor = new(0.35f, 0.55f, 0.95f, 0.18f);
        private static readonly Color SuccessTitleColor = new(0.36f, 0.76f, 0.46f);
        private static readonly Color SuccessSubtitleColor = new(0.5f, 0.5f, 0.5f);

        private static GUIStyle _sectionFoldoutStyle;
        private static GUIStyle _successTitleStyle;
        private static GUIStyle _successSubtitleStyle;
        private static Texture2D _warningBadgeTexture;
        private static Texture2D _neutralBadgeTexture;
        private static Texture _successTexture;
        private static bool _ready;

        /// <summary>
        /// Builds the shared styles once per domain. Call this at the top of OnGUI.
        /// </summary>
        public static void EnsureStyles()
        {
            if (_ready && _warningBadgeTexture != null)
                return;

            HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
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

            _warningBadgeTexture = MakeSolidTexture(WarningAccent);
            _neutralBadgeTexture = MakeSolidTexture(NeutralAccent);

            WarningBadgeStyle = MakeBadgeStyle(_warningBadgeTexture, new Color(0.15f, 0.13f, 0.05f));
            NeutralBadgeStyle = MakeBadgeStyle(_neutralBadgeTexture, Color.white);

            _successTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = SuccessTitleFontSize,
                normal =
                {
                    textColor = SuccessTitleColor
                },
                hover =
                {
                    textColor = SuccessTitleColor
                }
            };

            _successSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = SuccessSubtitleColor
                },
                hover =
                {
                    textColor = SuccessSubtitleColor
                }
            };

            _successTexture = EditorGUIUtility.IconContent("TestPassed").image;
            _ready = true;
        }

        public static GUIStyle BadgeStyle(EOverviewAccent accent) => accent == EOverviewAccent.Neutral
            ? NeutralBadgeStyle
            : WarningBadgeStyle;

        /// <summary>
        /// Draws a tinted, color coded section header with a count badge and returns the new expanded state.
        /// </summary>
        public static bool DrawSectionHeader(bool expanded, string label, int count, EOverviewAccent accent)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, SectionHeight, GUILayout.ExpandWidth(true));
            rect = new Rect(rect.x + 2f, rect.y + 4f, rect.width - 4f, rect.height);

            Color accentColor = AccentColor(accent);

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.16f));
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), accentColor);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f),
                    new Color(0f, 0f, 0f, 0.25f));
            }

            Rect foldoutRect = new(rect.x + 10f, rect.y + 3f, rect.width - 54f, rect.height - 6f);
            Rect badgeRect = new(rect.xMax - 36f, rect.y + (rect.height - BadgeHeight) * 0.5f, BadgeWidth,
                BadgeHeight);

            bool result = EditorGUI.Foldout(foldoutRect, expanded, label, true, _sectionFoldoutStyle);
            GUI.Label(badgeRect, count.ToString(), BadgeStyle(accent));

            return result;
        }

        /// <summary>
        /// Draws a drag strip that resizes the block above it and returns the new height.
        /// The height is clamped to the given range and stored under the EditorPrefs key when the drag ends.
        /// </summary>
        public static float DrawResizeHandle(float height, float minHeight, float maxHeight, string prefsKey)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, HandleHeight, GUILayout.ExpandWidth(true));
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            EventType type = Event.current.GetTypeForControl(controlId);
            bool active = GUIUtility.hotControl == controlId;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            if (type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.center.x - 14f, rect.center.y - 1f, 28f, 2f),
                    new Color(1f, 1f, 1f, active
                        ? 0.35f
                        : 0.14f));

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

        public static void DrawRowBackground(Rect rect, bool hovered, bool even)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            if (hovered)
                EditorGUI.DrawRect(rect, RowHoverColor);
            else if (even)
                EditorGUI.DrawRect(rect, RowEvenColor);
        }

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

        public static void Navigate(string assetPath)
        {
            Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);

            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static string Plural(int amount, string singular, string plural) => amount == 1
            ? singular
            : plural;

        public static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024L)
                return (bytes / (1024f * 1024f)).ToString("0.0") + " MB";

            if (bytes >= 1024L)
                return (bytes / 1024f).ToString("0.0") + " KB";

            return bytes + " B";
        }

        private static Color AccentColor(EOverviewAccent accent) => accent == EOverviewAccent.Neutral
            ? NeutralAccent
            : WarningAccent;

        private static GUIStyle MakeBadgeStyle(Texture2D background, Color textColor) => new(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = textColor,
                background = background
            }
        };

        private static Texture2D MakeSolidTexture(Color color)
        {
            Texture2D texture = new(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixel(0, 0, color);
            texture.Apply();

            return texture;
        }
    }
}