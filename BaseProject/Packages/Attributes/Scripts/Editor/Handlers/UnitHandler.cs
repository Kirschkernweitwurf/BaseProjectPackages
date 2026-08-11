using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the unit of a <see cref="UnitAttribute"/> at the right edge of the field, the same place
    /// <see cref="SuffixAttribute"/> puts its text.
    /// </summary>
    /// <remarks>
    /// An inline widget rather than a drawer, so a unit composes with whatever drawer the field already
    /// has. A slider or a progress bar keeps its own drawing and gains the unit beside it, which a
    /// second property drawer could not do.
    /// </remarks>
    internal sealed class UnitHandler : IInlineFieldWidget
    {
        private const float Padding = 4f;
        private const int WidgetOrder = 40;

        public int Order => WidgetOrder;

        public float GetWidth(in MemberContext context)
        {
            UnitAttribute attribute = context.GetAttribute<UnitAttribute>();
            if (attribute == null || string.IsNullOrEmpty(attribute.Unit))
                return 0f;

            return EditorStyles.miniLabel.CalcSize(ScratchContent.For(attribute.Unit)).x + Padding;
        }

        public void Draw(Rect rect, in MemberContext context)
        {
            UnitAttribute attribute = context.GetAttribute<UnitAttribute>();
            if (attribute == null)
                return;

            // GUI.Label rather than EditorGUI.LabelField: the overload taking a string and a style
            // reserves the inspector's label width for an empty prefix, which in a rect sized to the
            // unit itself leaves nothing to draw the unit in.
            using (new NoIndentScope())
            using (new EditorGUI.DisabledScope(true))
                GUI.Label(rect, attribute.Unit, EditorStyles.miniLabel);
        }
    }
}