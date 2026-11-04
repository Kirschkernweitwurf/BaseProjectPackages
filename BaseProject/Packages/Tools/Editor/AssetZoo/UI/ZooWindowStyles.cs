using Base.EditorUIPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// The styles only the Asset Zoo uses, on top of the shared window chrome. All text styles are white and take
    /// their real color from <see cref="GUI.contentColor"/>, so one style serves every state of a control.
    /// </summary>
    /// <remarks>
    /// Measure with the Pill styles. In Unity 6 CalcSize returns nothing on a style that clips with an ellipsis.
    /// </remarks>
    internal sealed class ZooWindowStyles : EditorWindowStyles
    {
        /// <summary>The bold name of a group in the Scene view.</summary>
        public GUIStyle PillGroup { get; private set; }

        /// <summary>The name of an asset in the Scene view.</summary>
        public GUIStyle PillName { get; private set; }

        /// <summary>The small line under a name in the Scene view.</summary>
        public GUIStyle PillDetail { get; private set; }

        /// <summary>The label of one part of a segmented control.</summary>
        public GUIStyle Segment { get; private set; }

        /// <summary>The name under a thumbnail, cut off when it does not fit.</summary>
        public GUIStyle TileName { get; private set; }

        /// <summary>The text of a folder chip.</summary>
        public GUIStyle Chip { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            base.Build();

            PillGroup = White(EditorStyles.boldLabel, TextClipping.Overflow);
            PillName = White(EditorStyles.label, TextClipping.Overflow);
            PillDetail = White(EditorStyles.miniLabel, TextClipping.Overflow);
            Segment = White(EditorStyles.label, TextClipping.Clip);
            Segment.alignment = TextAnchor.MiddleCenter;
            TileName = White(EditorStyles.miniLabel, TextClipping.Ellipsis);
            TileName.alignment = TextAnchor.UpperCenter;
            Chip = White(EditorStyles.label, TextClipping.Ellipsis);
            Chip.alignment = TextAnchor.MiddleLeft;
        }

        private static GUIStyle White(GUIStyle source, TextClipping clipping)
        {
            GUIStyle style = EditorStyleUtility.PinTextColor(new GUIStyle(source), Color.white);
            style.clipping = clipping;
            style.padding = new RectOffset();
            style.margin = new RectOffset();
            style.wordWrap = false;

            return style;
        }
    }
}