using System;
using Base.SaveSystemPackage.Storage;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the spelling of the keys a save is stored under. Two layers depend on it: one writes the
    /// keys, the other walks a flat listing and has to tell a live save apart from a kept backup of
    /// one. Getting that wrong makes every backup show up as a save of its own.
    /// </summary>
    public sealed class SaveKeysTests
    {
        private const string BackupId = "0000000000638500000";
        private const string SlotId = "slot_0";
        private const int TicksWidth = 19;

        /// <summary>A live key names the slot and the file inside it.</summary>
        [Test]
        public void ALiveKeyNamesTheSlotAndTheFile()
        {
            string key = SaveKeys.Key(SlotId, ESaveFile.Meta);

            Assert.That(key, Does.StartWith(SlotId));
            Assert.That(key, Does.Not.Contain(SaveKeys.BackupFolderPrefix));
        }

        /// <summary>Each part of a save gets its own key.</summary>
        [Test]
        public void EachFileOfASaveHasItsOwnKey()
        {
            string data = SaveKeys.Key(SlotId, ESaveFile.Data);
            string meta = SaveKeys.Key(SlotId, ESaveFile.Meta);
            string screenshot = SaveKeys.Key(SlotId, ESaveFile.Screenshot);

            Assert.That(meta, Is.Not.EqualTo(data));
            Assert.That(screenshot, Is.Not.EqualTo(data));
            Assert.That(screenshot, Is.Not.EqualTo(meta));
        }

        /// <summary>A file that does not exist has no key to build.</summary>
        [Test]
        public void AnUnknownFileIsRefused()
            => Assert.Throws<ArgumentOutOfRangeException>(() => SaveKeys.Key(SlotId, (ESaveFile)99));

        /// <summary>A backup key puts a generation folder between the slot and the file.</summary>
        [Test]
        public void ABackupKeySitsInsideItsGeneration()
        {
            string key = SaveKeys.BackupKey(SlotId, BackupId, ESaveFile.Meta);

            Assert.That(key, Does.StartWith(SaveKeys.BackupPrefix(SlotId)));
            Assert.That(key, Does.Contain(BackupId));
        }

        /// <summary>The backup prefix matches every backup key of its slot.</summary>
        [Test]
        public void TheBackupPrefixMatchesEveryBackupKey()
        {
            string prefix = SaveKeys.BackupPrefix(SlotId);

            Assert.That(SaveKeys.BackupKey(SlotId, BackupId, ESaveFile.Data), Does.StartWith(prefix));
            Assert.That(SaveKeys.BackupKey(SlotId, BackupId, ESaveFile.Meta), Does.StartWith(prefix));
        }

        /// <summary>A backup id is fixed width, so ids sort chronologically as plain text.</summary>
        [Test]
        public void ABackupIdIsFixedWidth()
        {
            string early = SaveKeys.CreateBackupId(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            string late = SaveKeys.CreateBackupId(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.That(early.Length, Is.EqualTo(TicksWidth));
            Assert.That(late.Length, Is.EqualTo(TicksWidth));
            Assert.That(string.CompareOrdinal(early, late), Is.LessThan(0));
        }

        /// <summary>A backup id reads back as the moment it was made for.</summary>
        [Test]
        public void ABackupIdReadsBackAsItsMoment()
        {
            DateTime moment = new(2024, 5, 17, 13, 45, 30, DateTimeKind.Utc);

            Assert.That(SaveKeys.ToCreationUtc(SaveKeys.CreateBackupId(moment)), Is.EqualTo(moment));
        }

        /// <summary>Anything that is not a timestamp sorts oldest, so it gets pruned away.</summary>
        [Test]
        public void SomethingThatIsNotATimestampSortsOldest()
        {
            Assert.That(SaveKeys.ToCreationUtc(null), Is.EqualTo(DateTime.MinValue));
            Assert.That(SaveKeys.ToCreationUtc("not a timestamp"), Is.EqualTo(DateTime.MinValue));
            Assert.That(SaveKeys.ToCreationUtc("-1"), Is.EqualTo(DateTime.MinValue));
            Assert.That(SaveKeys.ToCreationUtc("99999999999999999999"), Is.EqualTo(DateTime.MinValue));
        }

        /// <summary>Only the metadata file is the commit marker of a save.</summary>
        [Test]
        public void OnlyTheMetadataFileIsAMarker()
        {
            Assert.That(SaveKeys.IsMetaKey(SaveKeys.Key(SlotId, ESaveFile.Meta)), Is.True);
            Assert.That(SaveKeys.IsMetaKey(SaveKeys.Key(SlotId, ESaveFile.Data)), Is.False);
            Assert.That(SaveKeys.IsMetaKey(null), Is.False);
        }

        /// <summary>The marker of a live save names the slot it belongs to.</summary>
        [Test]
        public void ALiveMarkerNamesItsSlot()
        {
            Assert.That(SaveKeys.TryGetLiveSlotId(SaveKeys.Key(SlotId, ESaveFile.Meta), out string found), Is.True);
            Assert.That(found, Is.EqualTo(SlotId));
        }

        /// <summary>
        /// A backup's marker is not a live one. This is what keeps kept generations out of a save
        /// listing that walks a flat set of keys.
        /// </summary>
        [Test]
        public void ABackupMarkerIsNotALiveOne()
        {
            string key = SaveKeys.BackupKey(SlotId, BackupId, ESaveFile.Meta);

            Assert.That(SaveKeys.TryGetLiveSlotId(key, out string found), Is.False);
            Assert.That(found, Is.Empty);
        }

        /// <summary>Anything that is not a marker names no slot.</summary>
        [Test]
        public void SomethingThatIsNotAMarkerNamesNoSlot()
        {
            Assert.That(SaveKeys.TryGetLiveSlotId(SaveKeys.Key(SlotId, ESaveFile.Data), out string _), Is.False);
            Assert.That(SaveKeys.TryGetLiveSlotId(null, out string _), Is.False);
        }

        /// <summary>A backup key names the generation it belongs to.</summary>
        [Test]
        public void ABackupKeyNamesItsGeneration()
        {
            string key = SaveKeys.BackupKey(SlotId, BackupId, ESaveFile.Data);

            Assert.That(SaveKeys.TryGetBackupId(key, SlotId, out string found), Is.True);
            Assert.That(found, Is.EqualTo(BackupId));
        }

        /// <summary>A key belonging to another slot is not read as one of this slot's backups.</summary>
        [Test]
        public void AKeyOfAnotherSlotIsNotABackupOfThisOne()
        {
            string key = SaveKeys.BackupKey("slot_1", BackupId, ESaveFile.Data);

            Assert.That(SaveKeys.TryGetBackupId(key, SlotId, out string found), Is.False);
            Assert.That(found, Is.Empty);
        }

        /// <summary>A live key names no generation, because it is not in one.</summary>
        [Test]
        public void ALiveKeyNamesNoGeneration()
        {
            Assert.That(SaveKeys.TryGetBackupId(SaveKeys.Key(SlotId, ESaveFile.Data), SlotId, out string _), Is.False);
            Assert.That(SaveKeys.TryGetBackupId(null, SlotId, out string _), Is.False);
            Assert.That(SaveKeys.TryGetBackupId(SaveKeys.Key(SlotId, ESaveFile.Data), null, out string _), Is.False);
        }
    }
}