using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Draws the label of a <see cref="SuffixAttribute"/> after the value, as an inline widget rather
    /// than a property drawer.
    /// </summary>
    /// <remarks>
    /// A widget, because Unity runs exactly one property drawer per field. As a drawer the suffix
    /// competed with every attribute that draws the value itself, so combining it with a slider or a
    /// progress bar silently lost one of the two. A widget is drawn beside whatever ends up owning the
    /// field, so it composes with all of them.
    /// </remarks>
    internal sealed class SuffixWidget : IInlineFieldWidget
    {
        private const float Padding = 4f;
        private const int WidgetOrder = 90;

        /// <inheritdoc/>
        public int Order => WidgetOrder;

        /// <inheritdoc/>
        public float GetWidth(in MemberContext context)
        {
            SuffixAttribute suffix = context.GetAttribute<SuffixAttribute>();

            if (suffix == null || string.IsNullOrEmpty(suffix.Text))
                return 0f;

            return EditorStyles.label.CalcSize(ScratchContent.For(suffix.Text)).x + Padding;
        }

        /// <inheritdoc/>
        public void Draw(Rect rect, in MemberContext context)
        {
            SuffixAttribute suffix = context.GetAttribute<SuffixAttribute>();

            if (suffix == null || string.IsNullOrEmpty(suffix.Text))
                return;

            Rect label = new(rect.x + Padding, rect.y, rect.width - Padding, rect.height);

            GUI.Label(label, suffix.Text, EditorStyles.label);
        }
    }
}