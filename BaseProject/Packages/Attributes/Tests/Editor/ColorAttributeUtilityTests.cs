using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers where a title or a box gets its color from. Two ways of saying it exist because a preset
    /// reads better in code and a hex is the only way to match a palette exactly, and the drawer needs
    /// to know when neither was given so it can fall back to its own default.
    /// </summary>
    public sealed class ColorAttributeUtilityTests
    {
        private const string BareHex = "FF0000";
        private const string HashedHex = "#00FF00";
        private const string Unparseable = "not a color";

        /// <summary>A hex with a hash is read as written.</summary>
        [Test]
        public void AHexWithAHashIsRead()
        {
            Assert.That(ColorAttributeUtility.TryResolve(HashedHex, EColor.Default, out Color color), Is.True);
            Assert.That(color, Is.EqualTo(Color.green));
        }

        /// <summary>
        /// The hash is added when it is missing, since somebody copying a hex out of a design tool
        /// gets it either way.
        /// </summary>
        [Test]
        public void AHexWithoutAHashIsReadToo()
        {
            Assert.That(ColorAttributeUtility.TryResolve(BareHex, EColor.Default, out Color color), Is.True);
            Assert.That(color, Is.EqualTo(Color.red));
        }

        /// <summary>A preset is resolved when no hex was given.</summary>
        [Test]
        public void APresetIsResolvedOnItsOwn()
            => Assert.That(ColorAttributeUtility.TryResolve(null, EColor.Blue, out Color _), Is.True);

        /// <summary>
        /// A hex wins over a preset, so setting one on an attribute that already carries the other is
        /// an override rather than a conflict.
        /// </summary>
        [Test]
        public void AHexWinsOverAPreset()
        {
            ColorAttributeUtility.TryResolve(HashedHex, EColor.Red, out Color color);

            Assert.That(color, Is.EqualTo(Color.green));
        }

        /// <summary>
        /// The default preset is the absence of a choice, not a color, so it leaves the drawer to
        /// decide.
        /// </summary>
        [Test]
        public void TheDefaultPresetIsNotAChoice()
            => Assert.That(ColorAttributeUtility.TryResolve(null, EColor.Default, out Color _), Is.False);

        /// <summary>Neither form given means the drawer picks, which is the common case.</summary>
        [Test]
        public void NothingGivenResolvesToNothing()
        {
            Assert.That(ColorAttributeUtility.TryResolve(string.Empty, EColor.Default, out Color _), Is.False);
            Assert.That(ColorAttributeUtility.TryResolve(null, EColor.Default, out Color _), Is.False);
        }

        /// <summary>
        /// A hex that cannot be parsed falls through to the preset rather than being taken as a color,
        /// so a typo in one does not throw away the other.
        /// </summary>
        [Test]
        public void AnUnparseableHexFallsThroughToThePreset()
            => Assert.That(ColorAttributeUtility.TryResolve(Unparseable, EColor.Blue, out Color _), Is.True);

        /// <summary>A typo with nothing behind it resolves to nothing rather than to black.</summary>
        [Test]
        public void AnUnparseableHexAloneResolvesToNothing()
            => Assert.That(ColorAttributeUtility.TryResolve(Unparseable, EColor.Default, out Color _), Is.False);
    }
}