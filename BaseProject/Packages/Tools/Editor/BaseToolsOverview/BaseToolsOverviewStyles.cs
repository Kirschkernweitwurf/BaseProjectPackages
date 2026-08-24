using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.BaseToolsOverview
{
    /// <summary>
    /// The styles the Base Tools overview draws its rows with, built on the shared editor look
    /// so the page matches the Base windows rather than inventing a second one.
    /// </summary>
    internal sealed class BaseToolsOverviewStyles : EditorStyleSet
    {
        private const int IntroFontSize = 11;
        private const int NameFontSize = 12;

        /// <summary>The sentence above the list.</summary>
        internal GUIStyle Intro { get; private set; }

        /// <summary>The page name in a row.</summary>
        internal GUIStyle Name { get; private set; }

        /// <summary>The button that jumps to a page.</summary>
        internal GUIStyle OpenButton { get; private set; }

        /// <summary>The description under a page name.</summary>
        internal GUIStyle Summary { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            Intro = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                fontSize = IntroFontSize,
                wordWrap = true
            }, EditorPalette.DimText);

            Name = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis,
                fontSize = NameFontSize,
                padding = new RectOffset()
            }, EditorPalette.Text);

            Summary = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis,
                padding = new RectOffset()
            }, EditorPalette.DimText);

            OpenButton = BuildOpenButton();
        }

        private GUIStyle BuildOpenButton()
        {
            GUIStyle style = new()
            {
                alignment = TextAnchor.MiddleCenter,
                border = EditorStyleUtility.UniformPadding(EditorMetrics.CardCornerRadius),
                fontSize = EditorStyles.miniLabel.fontSize,
                fontStyle = FontStyle.Bold
            };

            style.normal.background = Textures.Rounded(EditorPalette.Accent, EditorMetrics.CardCornerRadius);
            style.hover.background = Textures.Rounded(
                EditorStyleUtility.Shade(EditorPalette.Accent, true, false), EditorMetrics.CardCornerRadius);
            style.active.background = Textures.Rounded(
                EditorStyleUtility.Shade(EditorPalette.Accent, false, true), EditorMetrics.CardCornerRadius);
            style.focused.background = style.normal.background;

            return EditorStyleUtility.PinTextColor(style, EditorPalette.AccentText);
        }
    }
}