using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.TextPackage
{
    /// <summary>
    /// One choice of the font setting: which designed fonts it replaces and in which languages it can do that. A
    /// choice without swaps is the designed look. In a language the font has no letters for, the designed fonts stay,
    /// so a text never mixes two fonts.
    /// </summary>
    [Serializable]
    public sealed class ReadableFont
    {
        [Tooltip("Languages this font has every letter for. Empty means every language. In other languages the "
            + "designed fonts stay and the font setting shows the designed look instead of this font.")]
        [SerializeField] private List<Locale> locales = new();

        [Tooltip("Which designed font each readable font replaces. Designed fonts without a swap stay as they are.")]
        [SerializeField] private List<ReadableFontSwap> swaps = new();

        /// <summary>Whether this font can show texts in <paramref name="locale"/>.</summary>
        public bool CanShow(Locale locale)
        {
            if (locales.Count == 0)
                return true;

            if (locale == null)
                return false;

            foreach (Locale supported in locales)
            {
                if (supported != null
                    && supported.Identifier == locale.Identifier)
                    return true;
            }

            return false;
        }

        /// <summary>Finds the swap for <paramref name="font"/> drawn with <paramref name="material"/>.</summary>
        public bool TryGetSwap(TMP_FontAsset font, Material material, out ReadableFontSwap swap)
        {
            foreach (ReadableFontSwap candidate in swaps)
            {
                if (!candidate.IsFor(font, material))
                    continue;

                swap = candidate;
                return true;
            }

            swap = null;
            return false;
        }
    }
}