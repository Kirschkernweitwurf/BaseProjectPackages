using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Cached styles, colors and icons for the required-reference window.
    /// Pure presentation.
    /// </summary>
    public sealed class RequiredReferenceStyles
    {
        private const string AlertIcon = "console.erroricon.sml";
        private const string ObjectIcon = "GameObject Icon";
        private const string SuccessIcon = "TestPassed";
        private const int SummaryFontSize = 12;
        private const int TitleFontSize = 15;

        /// <summary>Accent used for problems.</summary>
        public static Color Accent => new(0.86f, 0.30f, 0.32f);

        /// <summary>Accent used for the all-good state.</summary>
        public static Color Success => new(0.36f, 0.76f, 0.46f);

        /// <summary>Subtle background behind a group header.</summary>
        public static Color Header => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.05f)
            : new Color(0f, 0f, 0f, 0.05f);

        /// <summary>Red alert icon shown per missing reference.</summary>
        public static Texture ErrorTexture => EditorGUIUtility.IconContent(AlertIcon).image;

        /// <summary>Green success icon shown in the empty state.</summary>
        public static Texture SuccessTexture => EditorGUIUtility.IconContent(SuccessIcon).image;

        /// <summary>Default object icon for a group header.</summary>
        public static Texture ObjectTexture => EditorGUIUtility.IconContent(ObjectIcon).image;

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
                }
            };

            SuccessSubtitle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal =
                {
                    textColor = new Color(0.5f, 0.5f, 0.5f)
                }
            };
        }
    }
}