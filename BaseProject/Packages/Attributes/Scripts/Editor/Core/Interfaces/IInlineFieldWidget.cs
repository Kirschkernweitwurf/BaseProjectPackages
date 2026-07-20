using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a small control on the field line, to the right of the value. The renderer reserves real
    /// width for these before drawing the field, so the field is narrower and the widget gets its own
    /// clicks. This avoids the click theft that happens when a button is only painted over the field.
    /// </summary>
    public interface IInlineFieldWidget
    {
        /// <summary>Left-to-right order within the trailing area. Lower sits closer to the field.</summary>
        int Order { get; }

        /// <summary>Width in pixels this widget needs, or zero when it does not apply to the field.</summary>
        float GetWidth(in MemberContext context);

        /// <summary>Draws the widget in the reserved rect.</summary>
        void Draw(Rect rect, in MemberContext context);
    }
}