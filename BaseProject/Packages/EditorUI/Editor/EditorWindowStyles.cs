using UnityEditor;
using UnityEngine;

namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The style set a Base editor window draws its chrome with: the name it carries at the top, the
    /// sentence under it, the headers of its sections and the line at its foot, on top of everything
    /// a list window already gets from <see cref="EditorTableStyles"/>.
    /// </summary>
    /// <remarks>
    /// This is what makes a window recognisable as one of the Base windows. A window with nothing to
    /// list still uses it, because the header, the card and the buttons are the signature rather than
    /// the table.
    /// <para>
    /// Building and releasing are inherited: call <c>EnsureBuilt</c> at the top of <c>OnGUI</c> and
    /// <c>Dispose</c> from <c>OnDisable</c>.
    /// </para>
    /// </remarks>
    public class EditorWindowStyles : EditorTableStyles
    {
        /// <summary>The sentence under the title, wrapped and dimmed.</summary>
        public GUIStyle Description { get; private set; }

        /// <summary>The line at the foot of a window, for a status or a count.</summary>
        public GUIStyle Footer { get; private set; }

        /// <summary>The header of one section of a window.</summary>
        public GUIStyle SectionHeader { get; private set; }

        /// <summary>The name the window carries at its top.</summary>
        public GUIStyle Title { get; private set; }

        /// <inheritdoc/>
        protected override void Build()
        {
            base.Build();

            Description = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.label)
            {
                fontSize = EditorMetrics.DescriptionFontSize,
                wordWrap = true
            }, EditorPalette.DimText);

            Footer = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft
            }, EditorPalette.DimText);

            SectionHeader = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel),
                EditorPalette.Text);

            Title = EditorStyleUtility.PinTextColor(new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = EditorMetrics.TitleFontSize
            }, EditorPalette.Text);
        }
    }
}