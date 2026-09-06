using System;
using System.Collections.Generic;
using Base.UtilityPackage.Identification;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the reason the key is a type rather than a string: the format is enforced once at
    /// construction, so nothing that would be unsafe to compose into a storage path can ever exist,
    /// and value equality makes it usable as a dictionary key.
    /// </summary>
    public sealed class PersistentKeyTests
    {
        private const string ValidKey = "Audio.MasterVolume";

        /// <summary>A well formed key keeps exactly the text it was given.</summary>
        [Test]
        public void AValidKeyKeepsItsText()
        {
            PersistentKey key = new(ValidKey);

            Assert.That(key.Value, Is.EqualTo(ValidKey));
            Assert.That(key.IsEmpty, Is.False);
        }

        /// <summary>An empty key carries nothing, so it has to be refused.</summary>
        [Test]
        public void AnEmptyKeyIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PersistentKey(null));
            Assert.Throws<ArgumentException>(() => new PersistentKey(string.Empty));
        }

        /// <summary>Whitespace at either end is invisible in a log and has to be refused.</summary>
        [Test]
        public void SurroundingWhitespaceIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PersistentKey(" " + ValidKey));
            Assert.Throws<ArgumentException>(() => new PersistentKey(ValidKey + " "));
            Assert.Throws<ArgumentException>(() => new PersistentKey("\t" + ValidKey));
        }

        /// <summary>A separator or a quote would break the storage key it is composed into.</summary>
        [Test]
        public void SeparatorsAndQuotesAreRefused()
        {
            Assert.That(PersistentKey.IsValid("Audio/Master"), Is.False);
            Assert.That(PersistentKey.IsValid("Audio\\Master"), Is.False);
            Assert.That(PersistentKey.IsValid("Audio\"Master"), Is.False);
            Assert.That(PersistentKey.IsValid("Audio'Master"), Is.False);
        }

        /// <summary>A control character has no place in a key that is written to disk.</summary>
        [Test]
        public void ControlCharactersAreRefused() => Assert.That(PersistentKey.IsValid("Audio\nMaster"), Is.False);

        /// <summary>An oversized key is an accident, not an intent, so it is refused.</summary>
        [Test]
        public void AnOversizedKeyIsRefused() => Assert.That(PersistentKey.IsValid(new string('a', 129)), Is.False);

        /// <summary>A key right at the limit is still fine.</summary>
        [Test]
        public void AKeyAtTheLengthLimitIsAccepted()
            => Assert.That(PersistentKey.IsValid(new string('a', 128)), Is.True);

        /// <summary>The non throwing path reports the failure instead of raising it.</summary>
        [Test]
        public void TryCreateReportsAnInvalidKey()
        {
            Assert.That(PersistentKey.TryCreate(" bad", out PersistentKey key), Is.False);
            Assert.That(key.IsEmpty, Is.True);
        }

        /// <summary>The non throwing path hands back a usable key for valid text.</summary>
        [Test]
        public void TryCreateHandsBackAValidKey()
        {
            Assert.That(PersistentKey.TryCreate(ValidKey, out PersistentKey key), Is.True);
            Assert.That(key.Value, Is.EqualTo(ValidKey));
        }

        /// <summary>Two keys built from the same text have to count as the same key.</summary>
        [Test]
        public void KeysWithTheSameTextAreEqual()
        {
            PersistentKey first = new(ValidKey);
            PersistentKey second = new(ValidKey);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(second.GetHashCode(), Is.EqualTo(first.GetHashCode()));
        }

        /// <summary>Case is part of the key, so two spellings are two keys.</summary>
        [Test]
        public void KeysWithDifferentCaseAreNotEqual()
            => Assert.That(new PersistentKey("audio"), Is.Not.EqualTo(new PersistentKey("Audio")));

        /// <summary>Value equality is what makes the key safe to look entries up with.</summary>
        [Test]
        public void AKeyCanAddressADictionaryEntry()
        {
            Dictionary<PersistentKey, int> entries = new()
            {
                [new PersistentKey(ValidKey)] = 42
            };

            Assert.That(entries[new PersistentKey(ValidKey)], Is.EqualTo(42));
        }

        /// <summary>The default value carries no key and has to say so without throwing.</summary>
        [Test]
        public void TheDefaultKeyIsEmpty()
        {
            PersistentKey key = default;

            Assert.That(key.IsEmpty, Is.True);
            Assert.That(key.Value, Is.Null);
            Assert.That(key.ToString(), Is.Empty);
            Assert.That(key.GetHashCode(), Is.EqualTo(0));
        }
    }
}