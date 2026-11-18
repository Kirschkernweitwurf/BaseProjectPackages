using System;
using TMPro;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// The readable font and the extra spacing the player picked. Every <see cref="LocalizedTextBox"/> reads them when
    /// it applies its style and applies it again when they change, so one setting reaches every styled text.
    /// </summary>
    public static class ReadableText
    {
        /// <summary>
        /// The largest text size: the smallest text at 150 %, 24 px to 36 px at 1080p. The Xbox Accessibility
        /// Guidelines (XAG 101) ask for 200 % of the PC minimum of 18 px, which is 36 px, so a base size of 24 px
        /// reaches it at 150 %.
        /// </summary>
        public const float LargestScale = 1.5f;

        private const float DesignedScale = 1f;

        private static ReadableFont _font;
        private static TextSpacing _spacing;
        private static float _scale = DesignedScale;

        /// <summary>Raised when the font or the spacing changes, so texts can apply their style again.</summary>
        public static event Action Changed;

        /// <summary>
        /// Extra spacing on top of every styled paragraph's own, panel texts of more than two lines. Short texts keep
        /// their designed spacing.
        /// </summary>
        public static TextSpacing Spacing => _spacing;

        /// <summary>The player's text size as a factor of the smallest text's size; 1 is as designed.</summary>
        public static float Scale => _scale;

        /// <summary>
        /// Whether wider spacing or a bigger text size change every text. A readable font counts per box, since it
        /// only replaces the fonts it has a swap for.
        /// </summary>
        public static bool IsEnlarged => !_spacing.Equals(TextSpacing.None)
            || _scale > DesignedScale;

        /// <summary>Sets the font choice; null keeps the designed fonts.</summary>
        public static void SetFont(ReadableFont font)
        {
            if (font == _font)
                return;

            _font = font;
            Changed?.Invoke();
        }

        /// <summary>Sets the extra spacing; <see cref="TextSpacing.None"/> keeps the designed spacing.</summary>
        public static void SetSpacing(TextSpacing spacing)
        {
            if (spacing.Equals(_spacing))
                return;

            _spacing = spacing;
            Changed?.Invoke();
        }

        /// <summary>
        /// Sets the text size as a factor of the smallest text's size, from 1 (as designed) to
        /// <see cref="LargestScale"/>. Bigger texts grow by the same amount, see <see cref="GetScaledSize"/>.
        /// </summary>
        public static void SetScale(float scale)
        {
            float clamped = Mathf.Clamp(scale, DesignedScale, LargestScale);
            if (Mathf.Approximately(clamped, _scale))
                return;

            _scale = clamped;
            Changed?.Invoke();
        }

        /// <summary>
        /// The size a text designed at <paramref name="designed"/> shows at the player's text size. Every text grows by
        /// the same amount, the growth of its style's smallest readable size, so the smallest text doubles at the
        /// largest size while a big title grows far less and the hierarchy stays: what the Xbox Accessibility
        /// Guidelines ask (200 % of the minimum, not of every size) and how iOS and Android scale text.
        /// </summary>
        /// <param name="designed">The size the style sets.</param>
        /// <param name="smallest">The style's smallest readable size, the base of the growth.</param>
        public static float GetScaledSize(float designed, float smallest) => GetScaledSize(designed, smallest, _scale);

        /// <summary>
        /// The size a text designed at <paramref name="designed"/> shows at the text size <paramref name="scale"/>, for
        /// a text whose part of the screen zooms and so carries some of the growth itself.
        /// </summary>
        public static float GetScaledSize(float designed, float smallest, float scale)
            => designed + Mathf.Min(smallest, designed) * (scale - DesignedScale);

        /// <summary>
        /// Finds the readable replacement for <paramref name="font"/> drawn with <paramref name="material"/>, if a
        /// readable font is picked and can show the current language.
        /// </summary>
        public static bool TryGetSwap(TMP_FontAsset font, Material material, out ReadableFontSwap swap)
        {
            swap = null;
            if (_font == null
                || font == null)
                return false;

            return _font.CanShow(TextLocale.Current)
                && _font.TryGetSwap(font, material, out swap);
        }

        /// <summary>The readable replacement for <paramref name="font"/> or the font itself without one.</summary>
        public static TMP_FontAsset GetFont(TMP_FontAsset font)
        {
            if (font == null)
                return null;

            return TryGetSwap(font, font.material, out ReadableFontSwap swap)
                ? swap.ReadableFont
                : font;
        }

        /// <summary>
        /// Drops the choices without telling anyone, for when the settings go away at the end of a play session and the
        /// texts are going away with them.
        /// </summary>
        public static void Clear()
        {
            _font = null;
            _spacing = TextSpacing.None;
            _scale = DesignedScale;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Clear();
            Changed = null;
        }
    }
}