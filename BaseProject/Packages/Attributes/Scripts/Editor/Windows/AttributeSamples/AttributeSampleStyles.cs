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
        internal const float CategoryHeight = 22f;

        /// <summary>Height of one attribute row in the list.</summary>
        internal const float EntryHeight = 20f;

        /// <summary>Width of the buttons above the source block.</summary>
        internal const float ButtonWidth = 76f;

        /// <summary>Gap between the two panes.</summary>
        internal const float ColumnGap = 6f;

        /// <summary>Width of the list beside the content.</summary>
        internal const float ListWidth = 240f;

        /// <summary>Padding inside the content pane.</summary>
        internal const float Padding = 12f;

        /// <summary>Gap between two blocks in the content pane.</summary>
        internal const float SectionGap = 14f;

        private const int HeadingFontSize = 18;
        private const int SubheadingFontSize = 11;

        /// <summary>Large name of the selected attribute.</summary>
        internal GUIStyle Heading { get; }

        /// <summary>The line under it explaining what the attribute does.</summary>
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

        /// <summary>Builds the styles.</summary>
        internal AttributeSampleStyles()
        {
            Heading = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = HeadingFontSize,
                margin = new RectOffset(0, 0, 0, 2)
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

            // The category reads as a header rather than as a row: a foldout arrow, bold text, and its
            // own band of background so the eye can skip a whole group at once.
            Category = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(18, 4, 2, 2)
            };

            Entry = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(24, 6, 0, 0)
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
                padding = new RectOffset(10, 10, 8, 8)
            };

            Footer = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 4, 0, 0)
            };

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
                padding = new RectOffset(10, 10, 8, 8)
            };
        }
    }
}