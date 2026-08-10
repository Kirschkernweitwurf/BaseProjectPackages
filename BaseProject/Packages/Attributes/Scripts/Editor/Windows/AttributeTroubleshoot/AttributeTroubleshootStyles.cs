using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>Cached styles, colors and icons for the troubleshoot window. Pure presentation.</summary>
    public sealed class AttributeTroubleshootStyles
    {
        private const string ErrorIcon = "console.erroricon.sml";
        private const string ScriptIcon = "cs Script Icon";
        private const int SummaryFontSize = 12;
        private const string SuccessIcon = "TestPassed";
        private const int TitleFontSize = 15;
        private const string WarningIcon = "console.warnicon.sml";

        private static readonly Color Success = new(0.36f, 0.76f, 0.46f);

        private static readonly Color DarkHeader = new(1f, 1f, 1f, 0.05f);

        private static readonly Color LightHeader = new(0f, 0f, 0f, 0.05f);

        private static readonly Color SubtitleColor = new(0.5f, 0.5f, 0.5f);

        private static Texture _errorTexture;
        private static Texture _scriptTexture;
        private static Texture _successTexture;
        private static Texture _warningTexture;

        /// <summary>Subtle background behind a group header.</summary>
        public static Color Header => EditorGUIUtility.isProSkin
            ? DarkHeader
            : LightHeader;

        /// <summary>Red icon shown next to an error finding.</summary>
        public static Texture ErrorTexture => Resolve(ref _errorTexture, ErrorIcon);

        /// <summary>Yellow icon shown next to a warning finding.</summary>
        public static Texture WarningTexture => Resolve(ref _warningTexture, WarningIcon);

        /// <summary>Script icon shown in a group header.</summary>
        public static Texture ScriptTexture => Resolve(ref _scriptTexture, ScriptIcon);

        /// <summary>Green icon shown in the empty state.</summary>
        public static Texture SuccessTexture => Resolve(ref _successTexture, SuccessIcon);

        /// <summary>Bold label for the type name in a group header.</summary>
        public GUIStyle Name { get; private set; }

        /// <summary>Label for the member and attribute of a single finding.</summary>
        public GUIStyle Member { get; private set; }

        /// <summary>Muted label for the explanation of a single finding.</summary>
        public GUIStyle Message { get; private set; }

        /// <summary>Bold label used in the summary row under the action bar.</summary>
        public GUIStyle Summary { get; private set; }

        /// <summary>Large green title shown when nothing is wrong.</summary>
        public GUIStyle SuccessTitle { get; private set; }

        /// <summary>Muted subtitle shown under the success title.</summary>
        public GUIStyle SuccessSubtitle { get; private set; }

        /// <summary>Builds the GUI styles once. Must run inside a GUI callback.</summary>
        public void EnsureBuilt()
        {
            if (Name != null)
                return;

            Name = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Member = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Message = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                normal =
                {
                    textColor = SubtitleColor
                },
                hover =
                {
                    textColor = SubtitleColor
                }
            };

            Summary = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = SummaryFontSize
            };

            SuccessTitle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = TitleFontSize,
                normal =
                {
                    textColor = Success
                },
                hover =
                {
                    textColor = Success
                }
            };

            SuccessSubtitle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = SubtitleColor
                },
                hover =
                {
                    textColor = SubtitleColor
                }
            };
        }

        // Icons are only available inside a GUI callback, so they are resolved on first use.
        private static Texture Resolve(ref Texture cached, string iconName)
        {
            if (cached == null)
                cached = EditorGUIUtility.IconContent(iconName).image;

            return cached;
        }
    }
}
