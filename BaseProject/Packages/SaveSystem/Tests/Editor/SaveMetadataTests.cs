using System;
using Base.SaveSystemPackage.Model;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the record a load menu reads. It is immutable, so every update makes a copy, and the
    /// copy has to keep everything the caller did not ask to change.
    /// </summary>
    public sealed class SaveMetadataTests
    {
        private const string AppVersion = "1.2.3";
        private const int SaveVersion = 4;
        private const string SlotId = "slot_0";

        private DateTime _created;
        private SaveMetadata _metadata;

        /// <summary>Every test starts from fresh metadata for a brand new save.</summary>
        [SetUp]
        public void Build()
        {
            _created = new DateTime(2024, 5, 17, 13, 45, 30, DateTimeKind.Utc);
            _metadata = SaveMetadata.CreateNew(SlotId, SaveVersion, AppVersion, _created);
        }

        /// <summary>A brand new save carries no name, no play time and no thumbnail.</summary>
        [Test]
        public void ANewSaveStartsEmpty()
        {
            Assert.That(_metadata.SlotId, Is.EqualTo(SlotId));
            Assert.That(_metadata.SaveVersion, Is.EqualTo(SaveVersion));
            Assert.That(_metadata.AppVersion, Is.EqualTo(AppVersion));
            Assert.That(_metadata.DisplayName, Is.Null);
            Assert.That(_metadata.TotalPlayTime, Is.EqualTo(TimeSpan.Zero));
            Assert.That(_metadata.HasScreenshot, Is.False);
            Assert.That(_metadata.ScreenshotWidth, Is.EqualTo(0));
            Assert.That(_metadata.ScreenshotHeight, Is.EqualTo(0));
        }

        /// <summary>A brand new save was created and last written at the same moment.</summary>
        [Test]
        public void ANewSaveWasCreatedAndSavedAtOnce()
        {
            Assert.That(_metadata.CreatedUtc, Is.EqualTo(_created));
            Assert.That(_metadata.LastSavedUtc, Is.EqualTo(_created));
        }

        /// <summary>An update replaces only what it was given.</summary>
        [Test]
        public void AnUpdateReplacesOnlyWhatItWasGiven()
        {
            SaveMetadata updated = _metadata.With(displayName: "Chapter Two");

            Assert.That(updated.DisplayName, Is.EqualTo("Chapter Two"));
            Assert.That(updated.SaveVersion, Is.EqualTo(SaveVersion));
            Assert.That(updated.AppVersion, Is.EqualTo(AppVersion));
            Assert.That(updated.LastSavedUtc, Is.EqualTo(_created));
        }

        /// <summary>An update leaves the record it was made from untouched.</summary>
        [Test]
        public void AnUpdateLeavesTheOriginalAlone()
        {
            _metadata.With(displayName: "Chapter Two");

            Assert.That(_metadata.DisplayName, Is.Null);
        }

        /// <summary>The slot and the moment of creation are never rewritten.</summary>
        [Test]
        public void TheSlotAndCreationTimeAreNeverRewritten()
        {
            DateTime later = _created.AddHours(3);
            SaveMetadata updated = _metadata.With(lastSavedUtc: later);

            Assert.That(updated.SlotId, Is.EqualTo(SlotId));
            Assert.That(updated.CreatedUtc, Is.EqualTo(_created));
            Assert.That(updated.LastSavedUtc, Is.EqualTo(later));
        }

        /// <summary>Leaving a field out keeps the value that was already there.</summary>
        [Test]
        public void LeavingAFieldOutKeepsIt()
        {
            SaveMetadata named = _metadata.With("Chapter Two", totalPlayTime: TimeSpan.FromHours(2));
            SaveMetadata resaved = named.With(lastSavedUtc: _created.AddHours(1));

            Assert.That(resaved.DisplayName, Is.EqualTo("Chapter Two"));
            Assert.That(resaved.TotalPlayTime, Is.EqualTo(TimeSpan.FromHours(2)));
        }

        /// <summary>Recording a thumbnail keeps its size next to the flag.</summary>
        [Test]
        public void AThumbnailIsRecordedWithItsSize()
        {
            SaveMetadata updated = _metadata.With(hasScreenshot: true, screenshotWidth: 320, screenshotHeight: 180);

            Assert.That(updated.HasScreenshot, Is.True);
            Assert.That(updated.ScreenshotWidth, Is.EqualTo(320));
            Assert.That(updated.ScreenshotHeight, Is.EqualTo(180));
        }
    }
}