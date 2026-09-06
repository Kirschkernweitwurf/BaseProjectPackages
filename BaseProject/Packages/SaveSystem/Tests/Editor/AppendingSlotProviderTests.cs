using System;
using System.Globalization;
using Base.SaveSystemPackage.Slots;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the model where every save is a new entry. It never overwrites, and its ids lead with a
    /// timestamp so that listing newest first is a plain text sort.
    /// </summary>
    public sealed class AppendingSlotProviderTests
    {
        private const int SegmentCount = 3;
        private const int TimestampSegment = 1;
        private const char TimestampSeparator = '_';

        private AppendingSlotProvider _provider;

        /// <summary>Every test starts from a provider over an empty reader and writer.</summary>
        [SetUp]
        public void Build() => _provider = new AppendingSlotProvider(new SaveReaderProbe(), new SaveWriterProbe());

        /// <summary>The provider says which model it implements and that it can grow.</summary>
        [Test]
        public void TheModelIsAppendingAndCanGrow()
        {
            Assert.That(_provider.Model, Is.EqualTo(ESlotModel.Appending));
            Assert.That(_provider.SupportsNewSlots, Is.True);
        }

        /// <summary>Every save gets a slot of its own.</summary>
        [Test]
        public void EverySaveGetsAFreshSlot()
        {
            _provider.TryResolveSaveTarget(null, out string first);
            _provider.TryResolveSaveTarget(null, out string second);

            Assert.That(first, Is.Not.Empty);
            Assert.That(second, Is.Not.EqualTo(first));
        }

        /// <summary>A selection is deliberately ignored, because this model never overwrites.</summary>
        [Test]
        public void ASelectionIsIgnored()
        {
            Assert.That(_provider.TryResolveSaveTarget("an existing save", out string target), Is.True);
            Assert.That(target, Is.Not.EqualTo("an existing save"));
        }

        /// <summary>
        /// Ids are fixed width, so a plain text sort puts them in the order they were made.
        /// </summary>
        [Test]
        public void EveryIdIsTheSameWidth()
        {
            _provider.TryResolveSaveTarget(null, out string first);
            _provider.TryResolveSaveTarget(null, out string second);

            Assert.That(first, Has.Length.EqualTo(second.Length));
        }

        /// <summary>
        /// The timestamp an id leads with never runs backwards, which is what makes listing newest
        /// first a plain text sort.
        /// </summary>
        /// <remarks>
        /// The timestamp is compared rather than the whole id. Two saves made inside the same clock
        /// tick carry the same timestamp and are separated only by the random suffix behind it, so
        /// the whole id orders them arbitrarily while the timestamp still holds.
        /// </remarks>
        [Test]
        public void TheTimestampNeverRunsBackwards()
        {
            _provider.TryResolveSaveTarget(null, out string first);
            _provider.TryResolveSaveTarget(null, out string second);

            Assert.That(TimestampOf(second), Is.GreaterThanOrEqualTo(TimestampOf(first)));
        }

        /// <summary>A provider needs both halves: one to list saves, one to prune them.</summary>
        [Test]
        public void AReaderAndAWriterAreRequired()
        {
            Assert.Throws<ArgumentNullException>(() => new AppendingSlotProvider(null, new SaveWriterProbe()));
            Assert.Throws<ArgumentNullException>(() => new AppendingSlotProvider(new SaveReaderProbe(), null));
        }

        // An id reads as prefix, timestamp, unique suffix. Only the middle segment carries the time.
        private static long TimestampOf(string slotId)
        {
            string[] segments = slotId.Split(TimestampSeparator);

            Assert.That(segments, Has.Length.EqualTo(SegmentCount), $"'{slotId}' is not shaped as expected");

            return long.Parse(segments[TimestampSegment], CultureInfo.InvariantCulture);
        }
    }
}