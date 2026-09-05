using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Base.EditorUIPackage.Editor.Tests
{
    /// <summary>
    /// Covers the comparison that decides which preset a theme still counts as. It is written out one
    /// color at a time, so a clause that was pasted twice or left out reads exactly like the rest and
    /// shows up only as a settings page marking the wrong preset.
    /// </summary>
    /// <remarks>
    /// The set is built through the constructor by reflection rather than by hand, so a color added
    /// later is compared without this file being touched. If the comparison is not extended along with
    /// it, the new slot has nothing holding it and the sweep below fails naming that slot.
    /// </remarks>
    public sealed class EditorThemeColorsTests
    {
        private const float Alpha = 1f;
        private const int Unchanged = -1;

        private static readonly ConstructorInfo FullConstructor = FindFullConstructor();

        /// <summary>Two sets built the same way are the same set.</summary>
        [Test]
        public void TwoIdenticalSetsMatch()
            => Assert.That(Build(Unchanged).Matches(Build(Unchanged)), Is.True);

        /// <summary>A set matches itself, which is the case the settings page hits every repaint.</summary>
        [Test]
        public void ASetMatchesItself()
        {
            EditorThemeColors colors = Build(Unchanged);

            Assert.That(colors.Matches(colors), Is.True);
        }

        /// <summary>
        /// Nothing to compare against is not a match. A theme with no preset behind it must not be
        /// reported as still matching one.
        /// </summary>
        [Test]
        public void ComparingAgainstNothingDoesNotMatch()
            => Assert.That(Build(Unchanged).Matches(null), Is.False);

        /// <summary>
        /// Every color the constructor takes has to be one the comparison looks at. Any slot that
        /// still matches after being changed is a color the comparison forgot, and its position is
        /// named in the failure.
        /// </summary>
        [Test]
        public void EveryColorTakesPartInTheComparison()
        {
            EditorThemeColors original = Build(Unchanged);
            List<int> unnoticed = new();

            for (int index = 0; index < FullConstructor.GetParameters().Length; index++)
            {
                if (original.Matches(Build(index)))
                    unnoticed.Add(index);
            }

            Assert.That(unnoticed, Is.Empty);
        }

        /// <summary>The empty set the serializer uses compares as different from a filled one.</summary>
        [Test]
        public void AnEmptySetDoesNotMatchAFilledOne()
            => Assert.That(new EditorThemeColors().Matches(Build(Unchanged)), Is.False);

        /// <summary>
        /// Builds a set where every color differs from every other, optionally changing one of them to
        /// a value none of the others holds.
        /// </summary>
        private static EditorThemeColors Build(int changedIndex)
        {
            ParameterInfo[] parameters = FullConstructor.GetParameters();
            object[] arguments = new object[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
                arguments[index] = Shade(index);

            if (changedIndex >= 0)
                arguments[changedIndex] = Shade(parameters.Length + 1);

            return (EditorThemeColors)FullConstructor.Invoke(arguments);
        }

        /// <summary>A color unique to its position, so a swapped clause shows up as a mismatch.</summary>
        private static Color Shade(int index) => new(index / 64f, 1f - index / 64f, index / 128f, Alpha);

        /// <summary>The constructor that takes every color, as opposed to the empty one.</summary>
        private static ConstructorInfo FindFullConstructor()
        {
            ConstructorInfo widest = null;

            foreach (ConstructorInfo candidate in typeof(EditorThemeColors).GetConstructors())
            {
                if (widest == null || candidate.GetParameters().Length > widest.GetParameters().Length)
                    widest = candidate;
            }

            return widest;
        }
    }
}