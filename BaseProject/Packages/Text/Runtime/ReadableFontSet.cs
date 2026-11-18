using System.Collections.Generic;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// The choices of the font setting and the spacing of the wider spacing setting. The position of a font in the list
    /// is the stored setting value, so the first one is the designed look and the font carousel names them in the same
    /// order.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Text/New Readable Font Set", "RFS_ReadableFonts")]
    public sealed class ReadableFontSet : ScriptableObject
    {
        private const float WideCharacters = 12f;
        private const float WideLines = 32f;
        private const float WideParagraphs = 50f;
        private const float WideWords = 16f;

        [Tooltip("The font choices in menu order. The first is the designed look, without swaps.")]
        [SerializeField] private List<ReadableFont> fonts = new();

        [Tooltip("Position of the widest font in the list. The text fit check uses it, with wider spacing, as the "
            + "hardest case a player can pick.")]
        [SerializeField] private int widestFont;

        [Tooltip("Extra spacing the wider spacing setting adds to paragraphs, panel texts of more than two lines like "
            + "tooltips, in hundredths of the font size. The defaults reach the Xbox Accessibility "
            + "Guidelines' minimum for text blocks: letters 0.12 and words 0.16 of the font size, lines 1.5 times it."
            + " Paragraphs get less than their twice-the-line gap: line breaks in UI text mostly "
            + "split a label from its text, like a tooltip's subtitle and a big gap tore them apart. Short texts "
            + "like labels, buttons and cards keep their designed spacing, since they could only shrink or cut.")]
        [SerializeField] private TextSpacing wideSpacing = new(WideCharacters, WideWords, WideLines, WideParagraphs);

        /// <summary>How many font choices there are.</summary>
        public int Count => fonts.Count;

        /// <summary>Extra spacing the wider spacing setting adds to paragraphs.</summary>
        public TextSpacing WideSpacing => wideSpacing;

        /// <summary>The widest font choice, the hardest case for the text fit check.</summary>
        public ReadableFont WidestFont => GetFont(widestFont);

        /// <summary>The font choice at <paramref name="index"/>, clamped into the list; null if it is empty.</summary>
        public ReadableFont GetFont(int index) => fonts.Count == 0
            ? null
            : fonts[Mathf.Clamp(index, 0, fonts.Count - 1)];
    }
}