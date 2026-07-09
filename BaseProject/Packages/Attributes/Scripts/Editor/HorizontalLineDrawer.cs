using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the separator line for <see cref="HorizontalLineAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(HorizontalLineAttribute))]
    public sealed class HorizontalLineDrawer : DecoratorDrawer
    {
        private static readonly Color DefaultColor = new(0.5f, 0.5f, 0.5f, 1f);

        public override float GetHeight()
        {
            HorizontalLineAttribute line = (HorizontalLineAttribute)attribute;
            return line.Thickness + line.Padding * 2f;
        }

        public override void OnGUI(Rect position)
        {
            HorizontalLineAttribute line = (HorizontalLineAttribute)attribute;

            Color color = ColorAttributeUtility.TryResolve(line.ColorHex, line.PresetColor, out Color resolved)
                ? resolved
                : DefaultColor;

            Rect lineRect = new(position.x,
                position.y + line.Padding,
                position.width,
                line.Thickness);

            EditorGUI.DrawRect(lineRect, color);
        }
    }
}