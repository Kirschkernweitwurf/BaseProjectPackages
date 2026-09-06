using System;
using Base.SaveSystemPackage.Slots;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the model that hands out a fixed set of numbered slots. It never mints a new one, so a
    /// save has to land on a slot the player actually picked.
    /// </summary>
    /// <remarks>
    /// Only the synchronous half is covered here. Listing slots reads through the storage layer, which
    /// belongs to a play mode test rather than this one.
    /// </remarks>
    public sealed class FixedSlotProviderTests
    {
        private const int SlotCount = 3;

        private FixedSlotProvider _provider;

        /// <summary>Every test starts from a provider with three numbered slots.</summary>
        [SetUp]
        public void Build() => _provider = new FixedSlotProvider(new SaveReaderProbe(), SlotCount);

        /// <summary>The provider says which model it implements and that it cannot grow.</summary>
        [Test]
        public void TheModelIsFixedAndCannotGrow()
        {
            Assert.That(_provider.Model, Is.EqualTo(ESlotModel.Fixed));
            Assert.That(_provider.SupportsNewSlots, Is.False);
        }

        /// <summary>A selected slot is the slot that gets written.</summary>
        [Test]
        public void ASelectedSlotIsTheSaveTarget()
        {
            string selected = FixedSlotProvider.SlotId(1);

            Assert.That(_provider.TryResolveSaveTarget(selected, out string target), Is.True);
            Assert.That(target, Is.EqualTo(selected));
        }

        /// <summary>Every slot of the configured set can be written to.</summary>
        /// <param name="index">The slot being targeted.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void ASlotOfTheSetCanBeWrittenTo(int index)
            => Assert.That(_provider.TryResolveSaveTarget(FixedSlotProvider.SlotId(index), out string _), Is.True);

        /// <summary>A slot beyond the configured set does not exist and is refused.</summary>
        [Test]
        public void ASlotBeyondTheSetIsRefused() => Assert.That(
            _provider.TryResolveSaveTarget(FixedSlotProvider.SlotId(SlotCount), out string _),
            Is.False);

        /// <summary>Something that is not a slot id at all is refused.</summary>
        [Test]
        public void SomethingThatIsNotASlotIsRefused()
        {
            Assert.That(_provider.TryResolveSaveTarget("nonsense", out string _), Is.False);
            Assert.That(_provider.TryResolveSaveTarget(null, out string _), Is.False);
            Assert.That(_provider.TryResolveSaveTarget(string.Empty, out string _), Is.False);
        }

        /// <summary>Slot ids are built the same way every time, so a button can target one.</summary>
        [Test]
        public void SlotIdsAreStable()
        {
            Assert.That(FixedSlotProvider.SlotId(1), Is.EqualTo(FixedSlotProvider.SlotId(1)));
            Assert.That(FixedSlotProvider.SlotId(2), Is.Not.EqualTo(FixedSlotProvider.SlotId(1)));
        }

        /// <summary>A provider without a reader could never list its slots.</summary>
        [Test]
        public void AReaderIsRequired()
            => Assert.Throws<ArgumentNullException>(() => new FixedSlotProvider(null, SlotCount));

        /// <summary>A fixed model with no slots at all would have nowhere to save.</summary>
        [Test]
        public void AtLeastOneSlotIsRequired()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixedSlotProvider(new SaveReaderProbe(), 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FixedSlotProvider(new SaveReaderProbe(), -1));
        }
    }
}