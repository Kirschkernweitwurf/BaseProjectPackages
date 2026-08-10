using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// One drawn relation, keeping its endpoints so it can be restyled when the selection changes.
    /// <br/><br/>
    /// Emphasis is carried by opacity rather than by color. An unselected edge takes its color from the
    /// port it hangs off, and a node has one port serving every relation that leaves it. So nothing said
    /// about a single line survives the next redraw. Opacity is a plain element style the graph never
    /// touches, so it is the one channel that always holds.
    /// </summary>
    internal sealed class CodebaseGraphEdge : Edge
    {
        /// <summary>ID of the entry the relation starts at.</summary>
        public string SourceId { get; set; }

        /// <summary>ID of the entry the relation points at.</summary>
        public string TargetId { get; set; }

        /// <summary>How many usages back the relation up.</summary>
        public int Weight { get; set; }

        /// <summary>Applies an emphasis and asks for a redraw.</summary>
        /// <param name="color">Color to draw the line in, honored where the graph allows it.</param>
        /// <param name="width">Thickness to draw the line at.</param>
        /// <param name="opacity">How visible the line should be, which always holds.</param>
        public void Restyle(Color color, int width, float opacity)
        {
            style.opacity = opacity;

            edgeControl.inputColor = color;
            edgeControl.outputColor = color;
            edgeControl.edgeWidth = width;

            edgeControl.MarkDirtyRepaint();
            MarkDirtyRepaint();
        }
    }
}