using Base.EditorUiPackage;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot
{
    /// <summary>
    /// Cached styles, colors and icons for the findings list. Pure presentation.
    /// </summary>
    /// <remarks>
    /// Rebuilt when the editor skin or the active theme changes, since every color here is picked for
    /// one of them and the light and dark versions are not interchangeable.
    /// </remarks>
    internal sealed class AttributeTroubleshootStyles : EditorStyleSet
    {
        private const float HeaderStrength = 0.06f;
        private const float LightHeaderStrength = 0.05f;
        private const int SummaryFontSize = 12;
        private const int TitleFontSize = 15;

        /// <summary>Subtle background behind a group header.</summary>
        internal static Color Header => EditorPalette.Tint(HeaderStrength, LightHeaderStrength);

        /// <summary>The bar marking a finding that stops an attribute from working.</summary>
        internal static Color Error => EditorPalette.Danger;

        /// <summary>The bar marking a finding that only changes behavior.</summary>
        internal static Color Warning => EditorPalette.Warning;

        /// <summary>Red icon shown next to an error finding.</summary>
        internal static Texture ErrorTexture => EditorIcons.Error;

        /// <summary>Yellow icon shown next to a warning finding.</summary>
        internal static Texture WarningTexture => EditorIcons.Warning;

        /// <summary>Script icon shown in a group header.</summary>
        internal static Texture ScriptTexture => EditorIcons.Script;

        /// <summary>Green icon shown in the empty state.</summary>
        internal static Texture SuccessTexture => EditorIcons.Success;

        /// <summary>Tint of a row the pointer is over.</summary>
        internal Color Hover { get; private set; }

        /// <summary>Tint of every other finding, so a long group reads as rows.</summary>
        internal Color Stripe { get; private set; }

        /// <summary>The count shown at the right of a group header.</summary>
        internal GUIStyle Count { get; private set; }

        /// <summary>Label for the member and attribute of a single finding.</summary>
        internal GUIStyle Member { get; private set; }

        /// <summary>Muted label for the explanation of a single finding.</summary>
        internal GUIStyle Message { get; private set; }

        /// <summary>Bold label for the type name in a group header.</summary>
        internal GUIStyle Name { get; private set; }

        /// <summary>Bold label used in the summary row under the action bar.</summary>
        internal GUIStyle Summary { get; private set; }

        /// <summary>Muted subtitle shown under the success title.</summary>
        internal GUIStyle SuccessSubtitle { get; private set; }

        /// <summary>Large green title shown when nothing is wrong.</summary>
        internal GUIStyle SuccessTitle { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            Name = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Count = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            }, EditorStyleUtility.MutedTextColor());

            Member = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft
            };

            Message = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            }, EditorStyleUtility.MutedTextColor());

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

            Hover = EditorPalette.Hover;
            Stripe = EditorPalette.Stripe;
        }
    }
}