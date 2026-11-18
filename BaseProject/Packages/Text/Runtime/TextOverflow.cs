using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace Base.TextPackage
{
    /// <summary>
    /// How far a rendered text runs past its box, read from the outlines of the letters it drew. TextMeshPro's own
    /// <see cref="TMP_Text.isTextOverflowing"/> misses a one-line text that runs too wide, since it ignores the width
    /// while wrapping is off and its preferred size measures an auto-sized text at its largest size, not the size it
    /// settled on. The text fit check and any runtime overflow warning measure here, so they report what the
    /// player sees.
    /// </summary>
    /// <remarks>
    /// Letters are measured by their glyph outlines, without the padding of the distance field and without the empty
    /// room of the line above and below them, so a tight label whose letters sit inside its box fits, like it looks.
    /// </remarks>
    public static class TextOverflow
    {
        // A sliver past the box is the last letter's spacing or a line's padding, not a letter outside the box.
        private const float ToleranceFactor = 0.1f;

        /// <summary>
        /// How much wider and taller than its box, margins taken off, the letters of <paramref name="text"/> reach, in
        /// the text's own units: what runs past the left and the right edge together and past the top and the bottom.
        /// Zero on an axis that fits. Reads the last render, so update the mesh first.
        /// </summary>
        public static Vector2 Measure(TMP_Text text)
        {
            TMP_TextInfo info = text.textInfo;
            if (info == null)
                return Vector2.zero;

            float left = float.MaxValue;
            float right = float.MinValue;
            float bottom = float.MaxValue;
            float top = float.MinValue;
            bool hasLetters = false;

            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo character = info.characterInfo[i];
                if (!character.isVisible)
                    continue;

                GlyphMetrics glyph = character.textElement.glyph.metrics;
                float inkLeft = character.origin + glyph.horizontalBearingX * character.scale;
                float inkTop = character.baseLine + glyph.horizontalBearingY * character.scale;

                hasLetters = true;
                left = Mathf.Min(left, inkLeft);
                right = Mathf.Max(right, inkLeft + glyph.width * character.scale);
                bottom = Mathf.Min(bottom, inkTop - glyph.height * character.scale);
                top = Mathf.Max(top, inkTop);
            }

            if (!hasLetters)
                return Vector2.zero;

            Rect rect = text.rectTransform.rect;
            Vector4 margin = text.margin;
            float wide = Mathf.Max(0f, rect.xMin + margin.x - left) + Mathf.Max(0f, right - (rect.xMax - margin.z));
            float tall = Mathf.Max(0f, rect.yMin + margin.w - bottom) + Mathf.Max(0f, top - (rect.yMax - margin.y));

            return new Vector2(wide, tall);
        }

        /// <summary>How far <paramref name="text"/> may reach past its box and still count as fitting.</summary>
        public static float ToleranceOf(TMP_Text text) => text.fontSize * ToleranceFactor;

        /// <summary>True when the letters of <paramref name="text"/> run past its box by more than a sliver.</summary>
        public static bool IsOverflowing(TMP_Text text)
        {
            Vector2 spill = Measure(text);
            float tolerance = ToleranceOf(text);

            return spill.x > tolerance
                || spill.y > tolerance;
        }
    }
}