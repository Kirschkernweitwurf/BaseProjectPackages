using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>
    /// Cached styles, colors and icons for the required-reference window.
    /// Pure presentation.
    /// </summary>
    /// <remarks>
    /// Rebuilt when the editor skin or the active theme changes, since the styles pin text colors
    /// that are picked for one of them. The shared editor look comes from
    /// <see cref="EditorPalette"/>.
    /// </remarks>
    internal sealed class RequiredReferenceStyles : EditorStyleSet
    {
        private const float HeaderStrength = 0.05f;
        private const int TitleFontSize = 15;

        /// <summary>Subtle background behind a group header.</summary>
        internal static Color Header => EditorPalette.Tint(HeaderStrength);

        /// <summary>Red alert icon shown per missing reference.</summary>
        internal static Texture ErrorTexture => EditorIcons.Error;

        /// <summary>Green success icon shown in the empty state.</summary>
        internal static Texture SuccessTexture => EditorIcons.Success;

        /// <summary>Default object icon for a group header.</summary>
        internal static Texture ObjectTexture => EditorIcons.GameObject;

        /// <summary>Accent used for problems.</summary>
        internal static Color Accent => EditorPalette.Danger;

        /// <summary>Bold label for the object name in a group header.</summary>
        internal GUIStyle Name { get; private set; }

        /// <summary>Label for a single missing-reference path.</summary>
        internal GUIStyle Path { get; private set; }

        /// <summary>Centered white label used inside the count badge.</summary>
        internal GUIStyle Badge { get; private set; }

        /// <summary>Large green title shown when everything is assigned.</summary>
        internal GUIStyle SuccessTitle { get; private set; }

        /// <summary>Muted subtitle shown under the success title.</summary>
        internal GUIStyle SuccessSubtitle { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
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
        }
    }
}