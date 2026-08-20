using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Cached styles, colors and icons for the required-reference window.
    /// Pure presentation.
    /// </summary>
    /// <remarks>
    /// Rebuilt when the editor skin changes, since the styles pin text colors that are picked for
    /// one skin. The shared editor look comes from <see cref="EditorPalette"/>.
    /// </remarks>
    internal sealed class RequiredReferenceStyles
    {
        private const float HeaderStrength = 0.05f;
        private const int SummaryFontSize = 12;
        private const int TitleFontSize = 15;

        /// <summary>Subtle background behind a group header.</summary>
        public static Color Header => EditorPalette.Tint(HeaderStrength);

        /// <summary>Red alert icon shown per missing reference.</summary>
        public static Texture ErrorTexture => EditorIcons.Error;

        /// <summary>Green success icon shown in the empty state.</summary>
        public static Texture SuccessTexture => EditorIcons.Success;

        /// <summary>Default object icon for a group header.</summary>
        public static Texture ObjectTexture => EditorIcons.GameObject;

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
        public static Color Accent => EditorPalette.Danger;

        private bool _built;
        private bool _builtForProSkin;

        /// <summary>
        /// Builds the GUI styles once, and again after the editor skin changed.
        /// Must run inside a GUI callback.
        /// </summary>
        public void EnsureBuilt()
        {
            if (_built && _builtForProSkin == EditorGUIUtility.isProSkin)
                return;

            Name = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Path = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Badge = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            }, Color.white);

            Summary = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = SummaryFontSize
            };

            SuccessTitle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = TitleFontSize
            }, EditorPalette.Success);

            SuccessSubtitle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            }, EditorStyleUtility.MutedTextColor());

            _built = true;
            _builtForProSkin = EditorGUIUtility.isProSkin;
        }
    }
}