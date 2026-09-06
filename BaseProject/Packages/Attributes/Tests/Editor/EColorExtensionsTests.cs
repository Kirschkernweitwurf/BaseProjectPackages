using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the palette the attributes draw with. The colors end up next to each other in a single
    /// inspector, so two presets resolving to the same color would make them impossible to tell apart.
    /// </summary>
    public sealed class EColorExtensionsTests
    {
        private const float Tolerance = 0.001f;

        /// <summary>A color is fully opaque, since a translucent one would wash out what it sits on.</summary>
        /// <param name="preset">The preset under test.</param>
        [TestCaseSource(nameof(EveryPreset))]
        public void AColorIsFullyOpaque(EColor preset)
            => Assert.That(preset.ToColor().a, Is.EqualTo(1f).Within(Tolerance));

        /// <summary>
        /// Every named preset resolves to its own color. The unnamed one is left out, since it stands
        /// for no explicit choice and deliberately falls back onto another entry.
        /// </summary>
        [Test]
        public void EveryNamedPresetHasItsOwnColor() => Assert.That(NamedColors(), Is.Unique);

        /// <summary>Making no choice falls back to the neutral entry rather than to nothing.</summary>
        [Test]
        public void MakingNoChoiceFallsBackToWhite()
            => Assert.That(EColor.Default.ToColor(), Is.EqualTo(EColor.White.ToColor()));

        /// <summary>A preset this build does not know falls back to something visible.</summary>
        [Test]
        public void AnUnknownPresetFallsBackToWhite()
            => Assert.That(((EColor)byte.MaxValue).ToColor(), Is.EqualTo(EColor.White.ToColor()));

        /// <summary>The same preset always resolves to the same color.</summary>
        [Test]
        public void ThePaletteIsStable() => Assert.That(EColor.Blue.ToColor(), Is.EqualTo(EColor.Blue.ToColor()));

        /// <summary>Every preset the enum offers. One test case is generated per entry.</summary>
        private static IEnumerable<EColor> EveryPreset() => (EColor[])Enum.GetValues(typeof(EColor));

        // Gathering the colors here keeps the distinctness test down to the one thing it asserts.
        private static List<Color> NamedColors()
        {
            List<Color> colors = new();

            foreach (EColor preset in EveryPreset())
            {
                if (preset != EColor.Default)
                    colors.Add(preset.ToColor());
            }

            return colors;
        }
    }
}