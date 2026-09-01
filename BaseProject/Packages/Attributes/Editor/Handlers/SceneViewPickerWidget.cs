using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.SceneHandles;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Draws the arm button of <see cref="SceneViewPickerAttribute"/> next to the field. The scene view
    /// half lives in the handle drawer; this is only the switch that turns it on.
    /// </summary>
    internal sealed class SceneViewPickerWidget : IInlineFieldWidget
    {
        private const string ArmedLabel = "\u25A0";
        private const float ButtonWidth = 22f;
        private const string IdleLabel = "\u2316";
        private const string Tooltip = "Pick this reference by clicking in the scene view.";
        private const int WidgetOrder = 6;

        /// <inheritdoc/>
        public int Order => WidgetOrder;

        /// <inheritdoc/>
        public float GetWidth(in MemberContext context) => IsSupported(context)
            ? ButtonWidth
            : 0f;

        /// <inheritdoc/>
        public void Draw(Rect rect, in MemberContext context)
        {
            bool armed = ScenePickerState.IsArmedFor(context.Property);

            GUIContent content = new(armed
                ? ArmedLabel
                : IdleLabel, Tooltip);

            if (!GUI.Button(rect, content, EditorStyles.miniButton))
                return;

            if (armed)
            {
                ScenePickerState.Disarm();
                return;
            }

            ScenePickerState.Arm(context.Property);
            SceneView.RepaintAll();
        }

        private static bool IsSupported(in MemberContext context)
        {
            if (context.GetAttribute<SceneViewPickerAttribute>() == null)
                return false;

            return context.Property.propertyType == SerializedPropertyType.ObjectReference;
        }
    }
}