using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the bold title and underline for <see cref="TitleAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public sealed class TitleDrawer : DecoratorDrawer
    {
        private const float BottomSpacing = 4f;
        private const float LineSpacing = 2f;
        private const float LineThickness = 1f;
        private const float TitleHeight = 18f;
        private const float TopSpacing = 6f;

        public override float GetHeight() => TopSpacing + TitleHeight + LineSpacing + LineThickness + BottomSpacing;

        public override void OnGUI(Rect position)
        {
            TitleAttribute title = (TitleAttribute)attribute;
            bool hasColor = ColorAttributeUtility.TryResolve(title.ColorHex, title.PresetColor, out Color color);

            Rect labelRect = new(position.x, position.y + TopSpacing, position.width, TitleHeight);

            GUIStyle style = new(EditorStyles.boldLabel);
            if (hasColor)
                style.normal.textColor = color;

            EditorGUI.LabelField(labelRect, title.Title, style);

            Rect lineRect = new(position.x,
                labelRect.yMax + LineSpacing,
                position.width,
                LineThickness);

            Color lineColor = hasColor
                ? color
                : new Color(0.5f, 0.5f, 0.5f, 0.5f);

            EditorGUI.DrawRect(lineRect, lineColor);
        }
    }
}