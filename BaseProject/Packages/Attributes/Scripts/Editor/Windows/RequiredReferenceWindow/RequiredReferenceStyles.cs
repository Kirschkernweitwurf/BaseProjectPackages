using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Cached styles, colors and icons for the required-reference window.
    /// Pure presentation.
    /// </summary>
    internal sealed class RequiredReferenceStyles
    {
        private const string AlertIcon = "console.erroricon.sml";
        private const string ObjectIcon = "GameObject Icon";
        private const string SuccessIcon = "TestPassed";
        private const int SummaryFontSize = 12;
        private const int TitleFontSize = 15;

        /// <summary>Subtle background behind a group header.</summary>
        public static Color Header => EditorGUIUtility.isProSkin
            ? DarkHeader
            : LightHeader;

        /// <summary>Red alert icon shown per missing reference.</summary>
        public static Texture ErrorTexture => Resolve(ref _errorTexture, AlertIcon);

        /// <summary>Green success icon shown in the empty state.</summary>
        public static Texture SuccessTexture => Resolve(ref _successTexture, SuccessIcon);

        /// <summary>Default object icon for a group header.</summary>
        public static Texture ObjectTexture => Resolve(ref _objectTexture, ObjectIcon);

        /// <summary>Bold label for the object name in a group header.</summary>
        public GUIStyle Name { get; private set; }

        /// <summary>Label for a single missing-reference path.</summary>
        public GUIStyle Path { get; private set; }

        /// <summary>Centered white label used inside the count badge.</summary>
        public GUIStyle Badge { get; private set; }

        /// <summary>Bold label used in the summary row under the action bar.</summary>
        public GUIStyle Summary { get; private set; }

        /// <summary>Large green title shown when everything is assigned.</summary>
        public GUIStyle SuccessTitle { get; private set; }

        /// <summary>Muted subtitle shown under the success title.</summary>
        public GUIStyle SuccessSubtitle { get; private set; }

        /// <summary>Accent used for problems.</summary>
        public static readonly Color Accent = new(0.86f, 0.30f, 0.32f);

        private static readonly Color Success = new(0.36f, 0.76f, 0.46f);

        private static readonly Color DarkHeader = new(1f, 1f, 1f, 0.05f);

        private static readonly Color LightHeader = new(0f, 0f, 0f, 0.05f);

        private static readonly Color SubtitleColor = new(0.5f, 0.5f, 0.5f);

        private static Texture _errorTexture;
        private static Texture _objectTexture;
        private static Texture _successTexture;

        /// <summary>Builds the GUI styles once. Must run inside a GUI callback.</summary>
        public void EnsureBuilt()
        {
            if (Name != null)
                return;

            Name = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Path = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Badge = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
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