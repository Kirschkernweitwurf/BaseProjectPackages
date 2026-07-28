using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Shared drawing helpers for the menu overview windows. Rows, chips, headers and colors
    /// live here so the menu item window and the asset creation window stay identical.
    /// </summary>
    public static class MenuOverviewGui
    {
        /// <summary>Horizontal padding between columns.</summary>
        public const float Padding = 6f;
        /// <summary>Height of a single result row.</summary>
        public const float RowHeight = 24f;

        /// <summary>Width of the accent stripe drawn at the left edge of every row.</summary>
        public const float StripeWidth = 3f;

        private const float ChipInset = 3f;

        /// <summary>Centered mini label used inside a colored chip.</summary>
        public static GUIStyle ChipStyle => _chipStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            }
        };

        /// <summary>Label style for menu paths. Uses rich text to dim everything but the last segment.</summary>
        public static GUIStyle PathStyle => _pathStyle ??= new GUIStyle(EditorStyles.label)
        {
            richText = true
        };

        /// <summary>Right aligned bold style for priorities and orders.</summary>
        public static GUIStyle NumberStyle => _numberStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        /// <summary>Dimmed style for secondary columns such as members, types and file names.</summary>
        public static GUIStyle DetailStyle => _detailStyle ??= new GUIStyle(EditorStyles.miniLabel);

        /// <summary>Right aligned style for the origin badge.</summary>
        public static GUIStyle BadgeStyle => _badgeStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight
        };

        /// <summary>Centered style for the compact state marker.</summary>
        public static GUIStyle StateStyle => _stateStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter
        };

        /// <summary>Centered style that paints the marker of broken entries red.</summary>
        public static GUIStyle AlertStyle => _alertStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = MissingColor
            }
        };

        /// <summary>Right aligned toolbar counter.</summary>
        public static GUIStyle CountStyle => _countStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            fixedHeight = 0f
        };

        /// <summary>Left aligned footer hint.</summary>
        public static GUIStyle HintStyle => _hintStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            fixedHeight = 0f
        };

        private static Color MissingColor => new(0.92f, 0.42f, 0.38f);

        private static string DimHex => EditorGUIUtility.isProSkin
            ? "#8C8C8C"
            : "#6B6B6B";

        private static readonly GUIContent DynamicChip =
            new("dynamic", "Registered by the menu manager. Its path and priority can be changed there.");
        private static readonly GUIContent StaticChip =
            new("static", "Declared by a Unity attribute. Its path and priority live in the source file.");

        private static GUIStyle _chipStyle;
        private static GUIStyle _pathStyle;
        private static GUIStyle _numberStyle;
        private static GUIStyle _detailStyle;
        private static GUIStyle _badgeStyle;
        private static GUIStyle _stateStyle;
        private static GUIStyle _alertStyle;
        private static GUIStyle _countStyle;
        private static GUIStyle _hintStyle;

        /// <summary>Accent color of the stripe and the chip for a definition.</summary>
        public static Color AccentFor(EMenuDefinition definition) => definition == EMenuDefinition.Dynamic
            ? new Color(0.25f, 0.56f, 0.92f)
            : new Color(0.46f, 0.48f, 0.52f);

        /// <summary>Chip color, which turns red for broken entries and fades for disabled ones.</summary>
        public static Color ChipColor(EMenuDefinition definition, EMenuEntryState state) => state switch
        {
            EMenuEntryState.Missing => MissingColor,
            EMenuEntryState.Disabled => new Color(0.45f, 0.45f, 0.48f),
            _ => AccentFor(definition)
        };

        /// <summary>Chip label for a definition.</summary>
        public static GUIContent ChipContent(EMenuDefinition definition) => definition == EMenuDefinition.Dynamic
            ? DynamicChip
            : StaticChip;

        /// <summary>Draws the row background, the hover highlight and the accent stripe.</summary>
        public static void DrawRow(Rect row, int index, bool hover, Color accent)
        {
            if (hover)
                EditorGUI.DrawRect(row, HoverColor());
            else if ((index & 1) == 1)
                EditorGUI.DrawRect(row, StripeColor());

            EditorGUI.DrawRect(new Rect(row.x, row.y, StripeWidth, row.height), accent);
        }

        /// <summary>Draws the column header background with a separating bottom line.</summary>
        public static void DrawHeader(Rect row)
        {
            EditorGUI.DrawRect(row, HeaderColor());
            EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1f, row.width, 1f), LineColor());
        }

        /// <summary>Draws the footer background with a separating top line.</summary>
        public static void DrawFooter(Rect row)
        {
            EditorGUI.DrawRect(row, HeaderColor());
            EditorGUI.DrawRect(new Rect(row.x, row.y, row.width, 1f), LineColor());
        }

        /// <summary>Draws a filled chip with centered white text.</summary>
        public static void DrawChip(Rect rect, GUIContent content, Color color)
        {
            Rect chip = new(rect.x, rect.y + ChipInset, rect.width, Mathf.Max(0f, rect.height - ChipInset * 2f));
            EditorGUI.DrawRect(chip, color);
            GUI.Label(chip, content, ChipStyle);
        }

        /// <summary>
        /// Builds a rich text label that dims everything but the last path segment, so the eye
        /// lands on the actual entry name instead of the repeated parent folders.
        /// </summary>
        public static GUIContent PathContent(string path, string tooltip)
        {
            int separator = path.LastIndexOf('/');

            if (separator < 0)
                return new GUIContent(path, tooltip);

            string parent = path[..(separator + 1)];
            string leaf = path[(separator + 1)..];

            return new GUIContent($"<color={DimHex}>{parent}</color>{leaf}", tooltip);
        }

        /// <summary>Draws a label that behaves like a link. Returns true when it was clicked.</summary>
        public static bool DrawLink(Rect rect, GUIContent content, GUIStyle style)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return GUI.Button(rect, content, style);
        }

        private static Color HoverColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.07f)
            : new Color(0f, 0f, 0f, 0.06f);

        private static Color StripeColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.03f)
            : new Color(0f, 0f, 0f, 0.035f);

        private static Color HeaderColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.06f)
            : new Color(0f, 0f, 0f, 0.08f);

        private static Color LineColor() => EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.35f)
            : new Color(0f, 0f, 0f, 0.18f);
    }
}