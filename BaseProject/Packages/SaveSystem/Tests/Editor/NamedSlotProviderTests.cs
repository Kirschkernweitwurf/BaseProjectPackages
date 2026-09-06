using System;
using Base.SaveSystemPackage.Slots;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the model with unlimited named slots. With a slot selected a save overwrites it; without
    /// one it mints a fresh id, which is what makes "save as new" work.
    /// </summary>
    public sealed class NamedSlotProviderTests
    {
        private NamedSlotProvider _provider;

        /// <summary>Every test starts from a provider over an empty reader.</summary>
        [SetUp]
        public void Build() => _provider = new NamedSlotProvider(new SaveReaderProbe());

        /// <summary>The provider says which model it implements and that it can grow.</summary>
        [Test]
        public void TheModelIsNamedAndCanGrow()
        {
            Assert.That(_provider.Model, Is.EqualTo(ESlotModel.Named));
            Assert.That(_provider.SupportsNewSlots, Is.True);
        }

        /// <summary>A selected slot is overwritten rather than duplicated.</summary>
        [Test]
        public void ASelectedSlotIsOverwritten()
        {
            Assert.That(_provider.TryResolveSaveTarget("my save", out string target), Is.True);
            Assert.That(target, Is.EqualTo("my save"));
        }

        /// <summary>No selection means a fresh slot is minted.</summary>
        [Test]
        public void NoSelectionMintsAFreshSlot()
        {
            Assert.That(_provider.TryResolveSaveTarget(null, out string target), Is.True);
            Assert.That(target, Is.Not.Empty);
        }

        /// <summary>An empty selection counts as no selection.</summary>
        [Test]
        public void AnEmptySelectionCountsAsNone()
        {
            Assert.That(_provider.TryResolveSaveTarget(string.Empty, out string target), Is.True);
            Assert.That(target, Is.Not.Empty);
        }

        /// <summary>Two fresh slots never collide.</summary>
        [Test]
        public void FreshSlotsNeverCollide()
        {
            _provider.TryResolveSaveTarget(null, out string first);
            _provider.TryResolveSaveTarget(null, out string second);

            Assert.That(second, Is.Not.EqualTo(first));
        }

        /// <summary>A provider without a reader could never list its slots.</summary>
        [Test]
        public void AReaderIsRequired() => Assert.Throws<ArgumentNullException>(() => new NamedSlotProvider(null));
    }
}