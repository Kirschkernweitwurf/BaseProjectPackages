using System;
using System.Collections.Generic;
using System.Linq;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Checks that every text the sources know fits its box in every language: no overflow, no more lines than its
    /// style allows, no word too wide for the box and every glyph present. Styled boxes are measured at the locked
    /// size of their style; sizes are design values, so <see cref="SuggestSize"/> only advises.
    /// </summary>
    /// <remarks>
    /// One instance loads every prefab and scene with a text once and keeps the samples of each language, so a check
    /// followed by size suggestions does not open every scene again. Dispose closes the preview scenes.
    /// </remarks>
    internal sealed class TextFitValidator : IDisposable
    {
        private const string FailedFormat = "Text fit: {0} problems.\n{1}";
        private const string MissingStyleFormat = "No {0} style in the Text Styles table.";
        private const string NoEnglishText = "No English text, so the game shows nothing here. English is the fallback "
            + "of every language.";
        private const string PassedText = "Text fit: every text fits in every real language.";
        private const float SizeStep = 0.5f;
        private const string StyleArea = "Styles";
        private const float SuggestionCeilingFactor = 2f;
        private const string TightFormat =
            "\n{0} tight texts only fail in the pseudo-language (Text Fit window, Tight tab).";
        private const string TooSmallFormat = "Size {0} is below the readable minimum of {1}.";

        private readonly Dictionary<Locale, List<TextSample>> _samples = new();
        private readonly List<ITextSampleSource> _sources = new();
        private readonly TextFitStage _stage = new();

        /// <summary>How many texts the last <see cref="Validate"/> rendered, over every language.</summary>
        internal int SampleCount => _samples.Values.Sum(samples => samples.Count);

        /// <summary>Every box with words in it that no source checks, once each.</summary>
        internal List<UncheckedText> Unchecked { get; }

        /// <summary>Loads every prefab and scene with a text, then every source, into a fresh stage.</summary>
        internal TextFitValidator()
        {
            // A constructor that throws is never disposed, so the preview scenes are closed here.
            try
            {
                Load();
            }
            catch
            {
                _stage.Dispose();
                throw;
            }

            // Before anything renders, while the boxes still hold the text of their prefab or scene.
            Unchecked = _stage.Unclaimed();
        }

        /// <summary>Checks every text and writes the result to the console.</summary>
        internal static List<TextFitIssue> Run()
        {
            using TextFitValidator validator = new();
            List<TextFitIssue> issues = validator.Validate();
            Report(issues);

            return issues;
        }

        /// <summary>
        /// True when every text of a real language fits. Pseudo-locale problems are warnings and do not count.
        /// </summary>
        internal static bool IsShippable(List<TextFitIssue> issues) => issues.TrueForAll(issue => issue.IsPseudo);

        /// <summary>
        /// Writes the result to the console: one warning with every problem of a real language or one clean line.
        /// Pseudo-locale problems only show as a count, so a clean check stays short and a new problem stands out.
        /// </summary>
        internal static void Report(List<TextFitIssue> issues)
        {
            List<TextFitIssue> problems = issues.Where(issue => !issue.IsPseudo).ToList();
            int tightCount = issues.Count - problems.Count;
            string tight = tightCount == 0
                ? string.Empty
                : string.Format(TightFormat, tightCount);

            if (problems.Count == 0)
            {
                CustomLogger.Log(PassedText + tight, null);
                return;
            }

            CustomLogger.LogWarning(
                string.Format(FailedFormat, problems.Count, string.Join("\n", problems)) + tight, null);
        }

        /// <inheritdoc/>
        public void Dispose() => _stage.Dispose();

        /// <summary>
        /// Renders every sample in every language and returns what does not fit, once per text, box and language.
        /// </summary>
        internal List<TextFitIssue> Validate()
        {
            List<TextFitIssue> issues = new();

            foreach (Locale locale in TextFitLocales.All())
                issues.AddRange(MeasureIn(locale, () => ValidateIn(locale)));

            return issues;
        }

        /// <summary>Every Text Styles entry a box of any source uses, sorted.</summary>
        internal List<string> StyleKeys() => TextFitLocales.All()
            .Take(1)
            .SelectMany(SamplesOf)
            .Where(sample => sample.Box.HasStyle)
            .Select(sample => sample.Box.StyleKey)
            .Distinct()
            .OrderBy(key => key)
            .ToList();

        /// <summary>
        /// The largest size <see cref="SuggestSize"/> tries for <paramref name="style"/>. A suggestion of exactly this
        /// size means a larger one may fit too.
        /// </summary>
        internal static float CeilingOf(TextStyle style)
            => Mathf.Floor(style.FontSize * SuggestionCeilingFactor / SizeStep) * SizeStep;

        /// <summary>
        /// The largest size, in half steps, at which every text with the style <paramref name="key"/> fits in
        /// <paramref name="locale"/> or null when not even the readable minimum works. Advice only.
        /// </summary>
        internal float? SuggestSize(Locale locale, string key)
        {
            TextStyle style = TextStyleTable.For(locale, key);

            if (style == null)
                return null;

            List<TextSample> samples = SamplesOf(locale).Where(sample => sample.Box.StyleKey == key).ToList();
            bool checksWords = TextFitLocales.ChecksWords(locale);

            return MeasureIn(locale, () => LargestFit(samples, style, checksWords));
        }

        // Readable fonts and hyphens depend on the language and outside play mode no language is selected, so every
        // render names the one it measures.
        private static T MeasureIn<T>(Locale locale, Func<T> measure)
        {
            TextLocale.BeginMeasuring(TextFitLocales.SourceOf(locale));

            try
            {
                return measure();
            }
            finally
            {
                TextLocale.EndMeasuring();
            }
        }

        private static TextFitIssue StyleIssue(string code, TextBox box, string reason, TextStyle style)
        {
            TextSample sample = new(StyleArea, box, box.StyleKey, string.Empty, string.Empty, style);

            return new TextFitIssue(code, sample, reason);
        }

        // A prefab and every scene that holds it bring the same box, so the same text in a box of the same size,
        // font and style is only rendered and reported once, at the prefab, which loads first.
        private void Load()
        {
            TextFitFiles.LoadInto(_stage);

            foreach (Type type in TypeCache.GetTypesDerivedFrom<ITextSampleSource>())
            {
                if (type.IsAbstract)
                    continue;

                ITextSampleSource source = (ITextSampleSource)Activator.CreateInstance(type, true);
                source.Load(_stage);
                _sources.Add(source);
            }
        }

        private List<TextFitIssue> ValidateIn(Locale locale)
        {
            List<TextFitIssue> issues = new();
            Dictionary<string, TextStyle> styles = StylesOf(locale, issues);
            bool checksWords = TextFitLocales.ChecksWords(locale);
            HashSet<(TextBox, string)> failed = new();

            foreach (TextSample sample in SamplesOf(locale))
            {
                TextStyle style = null;

                // One failing set of Smart String values is enough to know the text needs work.
                if (failed.Contains((sample.Box, sample.Source))
                    || (sample.Box.HasStyle
                        && !styles.TryGetValue(sample.Box.StyleKey, out style)))
                    continue;

                float size = style == null
                    ? sample.Box.Text.fontSize
                    : style.FontSize;
                // The sources measure a missing translation in English, so only a text English lacks too is empty.
                string reason = string.IsNullOrEmpty(sample.Text)
                    ? NoEnglishText
                    : _stage.Check(sample, style, size, checksWords);

                if (reason == null)
                    continue;

                issues.Add(new TextFitIssue(TextFitLocales.CodeOf(locale), sample, reason));
                failed.Add((sample.Box, sample.Source));
            }

            return issues;
        }

        private float? LargestFit(List<TextSample> samples, TextStyle style, bool checksWords)
        {
            for (float size = CeilingOf(style); size >= style.MinReadableSize; size -= SizeStep)
            {
                if (samples.TrueForAll(sample => _stage.Check(sample, style, size, checksWords) == null))
                    return size;
            }

            return null;
        }

        private List<TextSample> SamplesOf(Locale locale)
        {
            if (_samples.TryGetValue(locale, out List<TextSample> samples))
                return samples;

            // The area tells places apart that look the same but give a text different room, like a hand card, which
            // grows on hover and a menu card, which does not.
            HashSet<(string, string, Vector2, TMP_FontAsset, float, string, Action)> seen = new();
            samples = _sources
                .SelectMany(source => source.Collect(locale))
                .Where(sample => seen.Add((sample.Area, sample.Text, sample.Box.Text.rectTransform.rect.size,
                    sample.Box.Text.font, sample.Box.Text.fontSize, sample.Box.StyleKey, sample.Arrange)))
                .ToList();
            _samples[locale] = samples;

            return samples;
        }

        private Dictionary<string, TextStyle> StylesOf(Locale locale, List<TextFitIssue> issues)
        {
            Dictionary<string, TextStyle> styles = new();
            string code = TextFitLocales.CodeOf(locale);

            foreach (TextBox box in SamplesOf(locale).Select(sample => sample.Box).Where(box => box.HasStyle))
            {
                if (styles.ContainsKey(box.StyleKey))
                    continue;

                TextStyle style = TextStyleTable.For(locale, box.StyleKey);

                if (style == null)
                {
                    issues.Add(StyleIssue(code, box, string.Format(MissingStyleFormat, box.StyleKey), null));
                    styles[box.StyleKey] = null;
                    continue;
                }

                if (style.SmallestSize < style.MinReadableSize)
                {
                    issues.Add(StyleIssue(code, box, string.Format(TooSmallFormat, style.SmallestSize,
                        style.MinReadableSize), style));
                }

                styles[box.StyleKey] = style;
            }

            // A missing style has no size to measure with, so its boxes are skipped instead of reported again.
            foreach (string missing in styles.Where(pair => pair.Value == null).Select(pair => pair.Key).ToList())
                styles.Remove(missing);

            return styles;
        }
    }
}