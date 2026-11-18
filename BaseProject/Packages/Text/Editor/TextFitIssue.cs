namespace Base.TextPackage.Editor
{
    /// <summary>A text that does not fit its box in one language or a style that is missing or too small.</summary>
    internal sealed class TextFitIssue
    {
        /// <summary>Creates the issue.</summary>
        /// <param name="locale">The language code, as the report shows it.</param>
        /// <param name="sample">The text that failed.</param>
        /// <param name="reason">Why it failed.</param>
        internal TextFitIssue(string locale, TextSample sample, string reason)
        {
            Locale = locale;
            Sample = sample;
            Reason = reason;
        }

        /// <summary>True for an issue of the pseudo-locale, which is a warning rather than a must-fix.</summary>
        internal bool IsPseudo => Locale == TextFitLocales.PseudoCode;

        /// <summary>The language code, as the report shows it.</summary>
        internal string Locale { get; }

        /// <summary>Why it failed.</summary>
        internal string Reason { get; }

        /// <summary>The text that failed.</summary>
        internal TextSample Sample { get; }

        public override string ToString() => string.IsNullOrEmpty(Sample.Arguments)
            ? $"[{Locale}] {Sample.Area} {Sample.Source}: {Reason} '{Sample.Text}'"
            : $"[{Locale}] {Sample.Area} {Sample.Source} ({Sample.Arguments}): {Reason} '{Sample.Text}'";
    }
}