using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Shared base for the small buttons that sit at the right edge of a field. Derived handlers only
    /// describe when they apply, what the button says and what a click does.
    /// </summary>
    internal abstract class InlineFieldButtonHandler : IInlineFieldWidget
    {
        /// <summary>Order within the trailing area. Lower sits closer to the field.</summary>
        protected abstract int InlineOrder { get; }

        /// <summary>Width the button reserves on the field line.</summary>
        protected abstract float InlineWidth { get; }

        int IInlineFieldWidget.Order => InlineOrder;

        /// <summary>Reserves width on the field line, or zero when the button does not apply.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>The width to reserve.</returns>
        public float GetWidth(in MemberContext context) => Applies(context) && IsSupported(context.Property)
            ? InlineWidth
            : 0f;

        /// <summary>Draws the button into the reserved rect.</summary>
        /// <param name="rect">The reserved rect.</param>
        /// <param name="context">The member currently being drawn.</param>
        public void Draw(Rect rect, in MemberContext context)
        {
            using (new EditorGUI.DisabledScope(!IsEnabled(context)))
            {
                if (FieldButtonRenderer.DrawAt(rect, GetContent(context)))
                    Execute(context);
            }
        }

        /// <summary>Whether this button belongs on the given member at all.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when the attribute is present and asks for an inline button.</returns>
        protected abstract bool Applies(in MemberContext context);

        /// <summary>Whether the button applies to the given property type.</summary>
        /// <param name="property">The property being drawn.</param>
        /// <returns>True when the property type is supported.</returns>
        protected abstract bool IsSupported(SerializedProperty property);

        /// <summary>Whether the button is clickable for the current value.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>True when the button should be enabled.</returns>
        protected abstract bool IsEnabled(in MemberContext context);

        /// <summary>Label and tooltip of the button.</summary>
        /// <param name="context">The member currently being drawn.</param>
        /// <returns>The button content.</returns>
        protected abstract GUIContent GetContent(in MemberContext context);

        /// <summary>Runs the button action.</summary>
        /// <param name="context">The member currently being drawn.</param>
        protected abstract void Execute(in MemberContext context);
    }
}