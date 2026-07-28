using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Shared base for the small field buttons (copy, clear, open). Draws the button inline at the
    /// right edge of the field, or on its own row below it when the attribute sets inline to false.
    /// Derived handlers only describe their attribute, their content and what a click does.
    /// </summary>
    public abstract class FieldButtonHandler : IInlineFieldWidget, IAfterFieldHandler
    {
        int IInlineFieldWidget.Order => InlineOrder;

        int IAfterFieldHandler.Order => RowOrder;

        /// <summary>Order of the inline button within the trailing area. Lower sits closer to the field.</summary>
        protected abstract int InlineOrder { get; }

        /// <summary>Order of the row button among the after-field handlers.</summary>
        protected abstract int RowOrder { get; }

        /// <summary>Width of the button while it sits on the field line.</summary>
        protected abstract float InlineWidth { get; }

        /// <summary>Width of the button while it sits on its own row.</summary>
        protected abstract float RowWidth { get; }

        /// <summary>Draws the button on its own row below the field, when the attribute asks for that.</summary>
        public void AfterField(in MemberContext context)
        {
            if (!TryGetPlacement(context, out bool inline) || inline)
                return;

            if (!IsSupported(context.Property))
                return;

            using (new EditorGUI.DisabledScope(!IsEnabled(context)))
            {
                if (FieldButtonRenderer.DrawRight(GetContent(context), RowWidth))
                    Execute(context);
            }
        }

        /// <summary>Reserves width on the field line, or zero when the button does not apply.</summary>
        public float GetWidth(in MemberContext context)
        {
            if (!TryGetPlacement(context, out bool inline) || !inline)
                return 0f;

            return IsSupported(context.Property)
                ? InlineWidth
                : 0f;
        }

        /// <summary>Draws the button into the reserved inline rect.</summary>
        public void Draw(Rect rect, in MemberContext context)
        {
            using (new EditorGUI.DisabledScope(!IsEnabled(context)))
            {
                if (FieldButtonRenderer.DrawAt(rect, GetContent(context)))
                    Execute(context);
            }
        }

        /// <summary>Returns false when the attribute is missing, otherwise reports where it draws.</summary>
        protected abstract bool TryGetPlacement(in MemberContext context, out bool inline);

        /// <summary>Whether the button applies to the given property type.</summary>
        protected abstract bool IsSupported(SerializedProperty property);

        /// <summary>Whether the button is clickable for the current value.</summary>
        protected abstract bool IsEnabled(in MemberContext context);

        /// <summary>Label and tooltip of the button.</summary>
        protected abstract GUIContent GetContent(in MemberContext context);

        /// <summary>Runs the button action.</summary>
        protected abstract void Execute(in MemberContext context);
    }
}