using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a compact single-line notice, much smaller than a default HelpBox. Supports info, warning
    /// and error styling and an optional custom color from a hex string or an <see cref="EColor"/> preset.
    /// </summary>
    public static class CompactHelpBox
    {
        private const string ErrorHex = "#DB4C52";
        private const string ErrorIcon = "console.erroricon.sml";

        private const float Height = 20f;
        private const float IconSize = 14f;
        private const string InfoHex = "#5A9BD4";

        private const string InfoIcon = "console.infoicon.sml";
        private const string NeutralHex = "#7F7F7F";
        private const float Padding = 4f;
        private const float TintAlpha = 0.10f;
        private const string WarningHex = "#E0A020";
        private const string WarningIcon = "console.warnicon.sml";

        private static GUIStyle _label;

        /// <summary>Draws a compact info line.</summary>
        public static void Info(string message) => Draw(message, EInfoBoxType.Info);

        /// <summary>Draws a compact warning line.</summary>
        public static void Warning(string message) => Draw(message, EInfoBoxType.Warning);

        /// <summary>Draws a compact error line.</summary>
        public static void Error(string message) => Draw(message, EInfoBoxType.Error);

        /// <summary>Draws a compact line for the given type using its default color.</summary>
        public static void Draw(string message, EInfoBoxType type) => Draw(message, type, DefaultColor(type));

        /// <summary>
        /// Draws a compact line for the given type, resolving the color from a hex string or an
        /// <see cref="EColor"/> preset. Hex wins over the preset; neither falls back to the type color.
        /// </summary>
        public static void Draw(string message, EInfoBoxType type, string hex, EColor preset)
            => Draw(message, type, Resolve(hex, preset, DefaultColor(type)));

        /// <summary>Draws a compact line for the given type with an explicit color.</summary>
        public static void Draw(string message, EInfoBoxType type, Color color)
        {
            Build();

            Rect rect = EditorGUILayout.GetControlRect(false, Height);
            EditorGUI.DrawRect(rect, new Color(color.r, color.g, color.b, TintAlpha));

            Rect content = new(rect.x + Padding, rect.y, rect.width - Padding * 2f, rect.height);
            Texture icon = IconFor(type);

            if (icon != null)
            {
                Rect iconRect = new(content.x, content.y + (content.height - IconSize) * 0.5f, IconSize, IconSize);
                GUI.DrawTexture(iconRect, icon);
                content = new Rect(iconRect.xMax + Padding, content.y,
                    content.xMax - iconRect.xMax - Padding, content.height);
            }

            _label.normal.textColor = color;
            _label.hover.textColor = color;
            _label.active.textColor = color;
            _label.focused.textColor = color;
            _label.onNormal.textColor = color;
            _label.onHover.textColor = color;
            GUI.Label(content, message, _label);
        }

        private static Color Resolve(string hex, EColor preset, Color fallback)
        {
            if (!string.IsNullOrEmpty(hex))
            {
                string normalized = hex.StartsWith("#")
                    ? hex
                    : "#" + hex;

                if (ColorUtility.TryParseHtmlString(normalized, out Color parsed))
                    return parsed;
            }

            if (preset != EColor.Default)
                return preset.ToColor();

            return fallback;
        }

        private static Color DefaultColor(EInfoBoxType type)
        {
            switch (type)
            {
                case EInfoBoxType.Warning:
                    return FromHex(WarningHex);
                case EInfoBoxType.Error:
                    return FromHex(ErrorHex);
                case EInfoBoxType.Info:
                    return FromHex(InfoHex);
                default:
                    return FromHex(NeutralHex);
            }
        }

        private static Texture IconFor(EInfoBoxType type)
        {
            switch (type)
            {
                case EInfoBoxType.Info:
                    return EditorGUIUtility.IconContent(InfoIcon).image;
                case EInfoBoxType.Warning:
                    return EditorGUIUtility.IconContent(WarningIcon).image;
                case EInfoBoxType.Error:
                    return EditorGUIUtility.IconContent(ErrorIcon).image;
                default:
                    return null;
            }
        }

        private static Color FromHex(string hex) => ColorUtility.TryParseHtmlString(hex, out Color color)
            ? color
            : Color.gray;

        private static void Build()
        {
            if (_label != null)
                return;

            _label = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
        }
    }
}