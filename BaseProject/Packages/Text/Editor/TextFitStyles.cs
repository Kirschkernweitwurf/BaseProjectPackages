using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>The Base window styles, plus the headline of the Text Fit summary.</summary>
    internal sealed class TextFitStyles : EditorWindowStyles
    {
        private const int SummaryFontGrowth = 1;

        /// <summary>The line that says whether every text fits, a step larger than a row.</summary>
        internal GUIStyle SummaryTitle { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            base.Build();

            SummaryTitle = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Ellipsis,
                fontSize = EditorStyles.boldLabel.fontSize + SummaryFontGrowth
            }, EditorPalette.Text);
        }
    }
}