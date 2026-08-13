using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples
{
    /// <summary>Styles and metrics for the samples window, built once per window rather than per repaint.</summary>
    internal sealed class AttributeSampleStyles
    {
        /// <summary>Shown when nothing is selected.</summary>
        internal const string EmptyMessage = "Pick an attribute on the left.";

        /// <summary>Height of a category header in the list.</summary>
        internal const float CategoryHeight = 24f;

        /// <summary>Height of one attribute row in the list.</summary>
        internal const float EntryHeight = 21f;

        /// <summary>Width of the buttons above the source block.</summary>
        internal const float ButtonWidth = 76f;

        /// <summary>Gap between the two panes.</summary>
        internal const float ColumnGap = 8f;

        /// <summary>Width of the list beside the content.</summary>
        internal const float ListWidth = 265f;

        /// <summary>Padding inside the content pane.</summary>
        internal const float Padding = 14f;

        /// <summary>Gap between two blocks in the content pane.</summary>
        internal const float SectionGap = 16f;

        private const float HoverStrength = 0.06f;
        private const int HeadingFontSize = 19;
        private const int SubheadingFontSize = 11;
        private const float StripeStrength = 0.03f;

        /// <summary>Large name of the selected attribute.</summary>
        internal GUIStyle Heading { get; }

        /// <summary>The category the selected attribute belongs to, above its name.</summary>
        internal GUIStyle Eyebrow { get; }

        /// <summary>The line under the name explaining what the attribute does.</summary>
        internal GUIStyle Description { get; }

        /// <summary>Small heading above the preview and the source.</summary>
        internal GUIStyle Section { get; }

        /// <summary>Category header in the list.</summary>
        internal GUIStyle Category { get; }

        /// <summary>An unselected list row.</summary>
        internal GUIStyle Entry { get; }

        /// <summary>The selected list row.</summary>
        internal GUIStyle SelectedEntry { get; }

        /// <summary>Monospaced style for the source block.</summary>
        internal GUIStyle Source { get; }

        /// <summary>Background of the list column.</summary>
        internal GUIStyle ListBackground { get; }

        /// <summary>A block in the content pane.</summary>
        internal GUIStyle Card { get; }

        /// <summary>The attribute count under the list.</summary>
        internal GUIStyle Footer { get; }

        /// <summary>Tint of a row the pointer is over.</summary>
        internal Color Hover { get; }

        /// <summary>Tint of every other row, so a long category reads as rows.</summary>
        internal Color Stripe { get; }

        /// <summary>Line between the list and the content.</summary>
        internal Color Divider { get; }

        /// <summary>Builds the styles.</summary>
        internal AttributeSampleStyles()
        {
            bool pro = EditorGUIUtility.isProSkin;

            Heading = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = HeadingFontSize,
                margin = new RectOffset(0, 0, 0, 0)
            };

            Eyebrow = new GUIStyle(EditorStyles.miniLabel)
            {
                normal =
                {
                    textColor = EditorStyles.centeredGreyMiniLabel.normal.textColor
                }
            };

            Description = new GUIStyle(EditorStyles.label)
            {
                fontSize = SubheadingFontSize,
                wordWrap = true,
                normal =
                {
                    textColor = EditorStyles.centeredGreyMiniLabel.normal.textColor
                }
            };

            Section = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                padding = new RectOffset(0, 0, 0, 2)
            };

            Category = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(18, 4, 3, 3)
            };

            Entry = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(20, 6, 0, 0)
            };

            SelectedEntry = new GUIStyle(Entry)
            {
                fontStyle = FontStyle.Bold
            };

            ListBackground = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 4, 4)
            };

            Card = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 10, 10)
            };

            Footer = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 4, 0, 0)
            };

            Hover = pro
                ? new Color(1f, 1f, 1f, HoverStrength)
                : new Color(0f, 0f, 0f, HoverStrength);

            Stripe = pro
                ? new Color(1f, 1f, 1f, StripeStrength)
                : new Color(0f, 0f, 0f, StripeStrength);

            Divider = pro
                ? new Color(0f, 0f, 0f, 0.35f)
                : new Color(0f, 0f, 0f, 0.15f);

            // Monospaced, because source read in a proportional font loses the alignment that makes a
            // stack of attributes scannable.
            Source = new GUIStyle(EditorStyles.textArea)
            {
                font = Font.CreateDynamicFontFromOSFont(new[]
                {
                    "Consolas",
                    "Menlo",
                    "Monaco",
                    "Courier New"
                }, EditorStyles.textArea.fontSize),

                wordWrap = false,
                richText = false,
                padding = new RectOffset(12, 12, 10, 10)
            };
        }
    }
}