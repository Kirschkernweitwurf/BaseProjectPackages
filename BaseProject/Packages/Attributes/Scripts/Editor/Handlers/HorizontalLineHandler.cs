using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws the separator line for <see cref="HorizontalLineAttribute"/>.</summary>
    public sealed class HorizontalLineHandler : IBeforeFieldHandler
    {
        public int Order => 10;

        private static readonly Color DefaultColor = new(0.5f, 0.5f, 0.5f, 1f);

        public void BeforeField(in MemberContext context)
        {
            HorizontalLineAttribute attribute = context.GetAttribute<HorizontalLineAttribute>();
            if (attribute == null)
                return;

            Color color =
                ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color resolved)
                    ? resolved
                    : DefaultColor;

            Rect rect = EditorGUILayout.GetControlRect(false, attribute.Thickness + attribute.Padding * 2f);
            Rect line = new(rect.x, rect.y + attribute.Padding, rect.width, attribute.Thickness);
            EditorGUI.DrawRect(line, color);
        }
    }
}