using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Builds the labels every panel is made of. Four files each carried their own copy of the same
    /// three lines, which is the kind of duplication that survives because it is too small to notice
    /// and too spread out to see.
    /// </summary>
    internal static class GraphLabel
    {
        /// <summary>Creates a label carrying one style class.</summary>
        /// <param name="text">Text to show.</param>
        /// <param name="styleClass">Class the stylesheet targets.</param>
        /// <returns>The label.</returns>
        internal static Label Build(string text, string styleClass)
        {
            Label label = new(text);
            label.AddToClassList(styleClass);

            return label;
        }
    }
}