using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Base.EditorUIPackage.Editor.Tests
{
    /// <summary>
    /// Covers the five looks a project can start from. The palettes were fitted to a contrast target
    /// rather than picked by eye, and that target is the one thing about them a test can hold on to:
    /// a preset that drifts below it is unreadable no matter how good it looks in a screenshot.
    /// </summary>
    /// <remarks>
    /// The contrast cases are generated per preset and per editor skin, so a drift in one palette on
    /// one skin shows up as its own named failure rather than as one red line covering all ten.
    /// </remarks>
    public sealed class EditorThemePresetsTests
    {
        private const float BodyTextContrast = 4.5f;
        private const float LegibleTextContrast = 7f;
        private const int SwatchCount = 5;

        /// <summary>Every preset, once for the dark skin and once for the light one.</summary>
        private static IEnumerable<TestCaseData> EveryPresetAndSkin()
        {
            foreach (EEditorThemePreset preset in EditorThemePresets.CreateOrder())
            {
                yield return new TestCaseData(preset, true).SetName($"{preset} on the dark skin");
                yield return new TestCaseData(preset, false).SetName($"{preset} on the light skin");
            }
        }

        /// <summary>Every preset, for the checks that do not depend on the skin.</summary>
        private static IEnumerable<EEditorThemePreset> EveryPreset() => EditorThemePresets.CreateOrder();

        /// <summary>Body text clears the accessibility target against the window fill.</summary>
        /// <param name="preset">The palette under test.</param>
        /// <param name="isDarkMode">Which editor skin the palette is sampled for.</param>
        [TestCaseSource(nameof(EveryPresetAndSkin))]
        public void TextIsLegibleOnTheWindow(EEditorThemePreset preset, bool isDarkMode)
        {
            EditorThemeColors colors = EditorThemePresets.CreateColors(preset, isDarkMode);

            Assert.That(ContrastRatio.Between(colors.Text, colors.Background),
                Is.GreaterThanOrEqualTo(BodyTextContrast));
        }

        /// <summary>Body text clears the target against a card as well, not only the window.</summary>
        /// <param name="preset">The palette under test.</param>
        /// <param name="isDarkMode">Which editor skin the palette is sampled for.</param>
        [TestCaseSource(nameof(EveryPresetAndSkin))]
        public void TextIsLegibleOnACard(EEditorThemePreset preset, bool isDarkMode)
        {
            EditorThemeColors colors = EditorThemePresets.CreateColors(preset, isDarkMode);

            Assert.That(ContrastRatio.Between(colors.Text, colors.Card), Is.GreaterThanOrEqualTo(BodyTextContrast));
        }

        /// <summary>
        /// A label on the accent fill clears the target too, since that is where a primary button puts
        /// its text.
        /// </summary>
        /// <param name="preset">The palette under test.</param>
        /// <param name="isDarkMode">Which editor skin the palette is sampled for.</param>
        [TestCaseSource(nameof(EveryPresetAndSkin))]
        public void TheAccentLabelIsLegible(EEditorThemePreset preset, bool isDarkMode)
        {
            EditorThemeColors colors = EditorThemePresets.CreateColors(preset, isDarkMode);

            Assert.That(ContrastRatio.Between(colors.AccentText, colors.Accent),
                Is.GreaterThanOrEqualTo(BodyTextContrast));
        }

        /// <summary>The preset that promises to be the most legible reaches the higher target.</summary>
        /// <param name="isDarkMode">Which editor skin the palette is sampled for.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void TheMostLegiblePresetReachesTheHigherTarget(bool isDarkMode)
        {
            EditorThemeColors colors = EditorThemePresets.CreateColors(EEditorThemePreset.Ink, isDarkMode);

            Assert.That(ContrastRatio.Between(colors.Text, colors.Background),
                Is.GreaterThanOrEqualTo(LegibleTextContrast));
        }

        /// <summary>Every color is opaque, since a translucent one would wash out what it covers.</summary>
        /// <param name="preset">The palette under test.</param>
        /// <param name="isDarkMode">Which editor skin the palette is sampled for.</param>
        [TestCaseSource(nameof(EveryPresetAndSkin))]
        public void EveryColorIsFullyOpaque(EEditorThemePreset preset, bool isDarkMode)
        {
            EditorThemeColors colors = EditorThemePresets.CreateColors(preset, isDarkMode);

            Assert.That(new[] { colors.Background.a, colors.Card.a, colors.Text.a, colors.Accent.a },
                Is.All.EqualTo(1f).Within(0.001f));
        }

        /// <summary>Every preset carries a name and a description for the button that applies it.</summary>
        /// <param name="preset">The palette under test.</param>
        [TestCaseSource(nameof(EveryPreset))]
        public void EveryPresetIsNamedAndDescribed(EEditorThemePreset preset)
        {
            Assert.That(EditorThemePresets.DisplayName(preset), Is.Not.Empty);
            Assert.That(EditorThemePresets.Description(preset), Is.Not.Empty);
        }

        /// <summary>The swatch strip samples the five colors that say what a preset looks like.</summary>
        /// <param name="preset">The palette under test.</param>
        [TestCaseSource(nameof(EveryPreset))]
        public void TheSwatchStripSamplesFiveColors(EEditorThemePreset preset)
            => Assert.That(EditorThemePresets.CreateSwatches(preset, true), Has.Length.EqualTo(SwatchCount));

        /// <summary>The two editor skins get different palettes, not the same one twice.</summary>
        /// <param name="preset">The palette under test.</param>
        [TestCaseSource(nameof(EveryPreset))]
        public void TheTwoSkinsGetDifferentPalettes(EEditorThemePreset preset)
            => Assert.That(EditorThemePresets.CreateColors(preset, false).Background,
                Is.Not.EqualTo(EditorThemePresets.CreateColors(preset, true).Background));

        /// <summary>A dark palette is darker than its light counterpart, not merely different.</summary>
        /// <param name="preset">The palette under test.</param>
        [TestCaseSource(nameof(EveryPreset))]
        public void TheDarkPaletteIsTheDarkerOne(EEditorThemePreset preset)
            => Assert.That(Brightness(EditorThemePresets.CreateColors(preset, true).Background),
                Is.LessThan(Brightness(EditorThemePresets.CreateColors(preset, false).Background)));

        /// <summary>The order covers every preset exactly once, so none is unreachable in settings.</summary>
        [Test]
        public void TheOrderCoversEveryPresetOnce()
            => Assert.That(EditorThemePresets.CreateOrder(),
                Is.EquivalentTo((EEditorThemePreset[])Enum.GetValues(typeof(EEditorThemePreset))));

        /// <summary>Two different presets do not resolve to the same accent.</summary>
        [Test]
        public void TwoPresetsDoNotShareAnAccent()
        {
            List<Color> accents = new();

            foreach (EEditorThemePreset preset in EditorThemePresets.CreateOrder())
                accents.Add(EditorThemePresets.CreateColors(preset, true).Accent);

            Assert.That(accents, Is.Unique);
        }

        /// <summary>
        /// Each set of colors is a fresh object, so a caller that keeps one and changes it cannot
        /// reach into the preset every other window reads.
        /// </summary>
        [Test]
        public void EverySetOfColorsIsItsOwnObject()
            => Assert.That(EditorThemePresets.CreateColors(EEditorThemePreset.Slate, true),
                Is.Not.SameAs(EditorThemePresets.CreateColors(EEditorThemePreset.Slate, true)));

        /// <summary>The same preset always produces the same colors.</summary>
        [Test]
        public void ThePresetsAreStable()
            => Assert.That(EditorThemePresets.CreateColors(EEditorThemePreset.Harbor, true).Text,
                Is.EqualTo(EditorThemePresets.CreateColors(EEditorThemePreset.Harbor, true).Text));

        private static float Brightness(Color color) => color.r + color.g + color.b;
    }
}