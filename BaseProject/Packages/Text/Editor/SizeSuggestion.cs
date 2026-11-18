namespace Base.TextPackage.Editor
{
    /// <summary>
    /// The largest size at which every text with one style still fits in one language, next to the style that holds
    /// the size it has now. The current size is read live, so the row is right again after a size was applied.
    /// </summary>
    internal sealed class SizeSuggestion
    {
        /// <summary>Follows a capped size, since a larger one was never tried.</summary>
        internal const string OrMore = " or more";

        /// <summary>Creates the suggestion.</summary>
        /// <param name="locale">The language code, as the report shows it.</param>
        /// <param name="style">The style in that language.</param>
        /// <param name="fits">The largest fitting size or null when nothing fits down to the readable minimum.</param>
        /// <param name="isCapped">True when <paramref name="fits"/> is the largest size tried, so more may fit.</param>
        internal SizeSuggestion(string locale, TextStyle style, float? fits, bool isCapped)
        {
            Locale = locale;
            Style = style;
            Fits = fits;
            IsCapped = isCapped;
        }

        /// <summary>The largest fitting size or null when nothing fits down to the readable minimum.</summary>
        internal float? Fits { get; }

        /// <summary>True when <see cref="Fits"/> is the largest size tried, so a larger one may fit too.</summary>
        internal bool IsCapped { get; }

        /// <summary>True when the current size is no larger than the largest fitting one.</summary>
        internal bool IsFitting => Fits.HasValue
            && Fits.Value >= Size;

        /// <summary>The language code, as the report shows it.</summary>
        internal string Locale { get; }

        /// <summary>The locked size the style has now.</summary>
        internal float Size => Style.FontSize;

        /// <summary>The style in that language.</summary>
        internal TextStyle Style { get; }

        public override string ToString()
        {
            if (!Fits.HasValue)
                return $"[{Locale}] {Style.name}: now {Size}, nothing fits down to the readable minimum";

            string more = IsCapped
                ? OrMore
                : string.Empty;

            return $"[{Locale}] {Style.name}: now {Size}, fits up to {Fits.Value}{more}";
        }
    }
}