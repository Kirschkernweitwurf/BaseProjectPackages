using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Base.SaveSystemPackage.Encryption;
using Base.SaveSystemPackage.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the header the codec puts in front of every save. It is what lets one build read a file
    /// another one wrote, and what turns a damaged file into a clear refusal instead of a strange
    /// parse error further down.
    /// </summary>
    public sealed class SaveCodecTests
    {
        private const int AlgorithmOffset = 4;
        private const int ChecksumOffset = 5;
        private const byte ExpectedFormatVersion = 2;
        private const int FormatVersionOffset = 3;
        private const int HeaderSize = 9;
        private const byte LegacyFormatVersion = 1;
        private const int LegacyHeaderSize = 5;
        private const string OtherPassphrase = "a different passphrase";
        private const string Passphrase = "the passphrase";
        private const string ProbeLabel = "Chapter Two";
        private const int ProbeScore = 42;

        private static readonly byte[] Magic = { (byte)'B', (byte)'S', (byte)'V' };

        private static AesEncryptor _encryptor;
        private static AesEncryptor _otherEncryptor;

        private SaveCodec _codec;

        /// <summary>
        /// Deriving a key runs a hundred thousand hashing rounds, so both are derived once for the
        /// whole fixture rather than inside the tests that need them.
        /// </summary>
        [OneTimeSetUp]
        public void BuildKeys()
        {
            _encryptor = new AesEncryptor(Passphrase);
            _otherEncryptor = new AesEncryptor(OtherPassphrase);
        }

        /// <summary>Every test starts from a plain codec that neither encrypts nor pretty prints.</summary>
        [SetUp]
        public void Build() => _codec = PlainCodec();

        /// <summary>What was encoded comes back as the same values.</summary>
        [Test]
        public void APayloadSurvivesTheRoundTrip()
        {
            byte[] encoded = _codec.Encode(Payload());

            SaveProbePayload decoded = _codec.Decode<SaveProbePayload>(encoded);

            Assert.That(decoded.label, Is.EqualTo(ProbeLabel));
            Assert.That(decoded.score, Is.EqualTo(ProbeScore));
        }

        /// <summary>The header identifies the file, its layout and how it was encrypted.</summary>
        [Test]
        public void TheHeaderIdentifiesTheFile()
        {
            byte[] encoded = _codec.Encode(Payload());

            Assert.That(encoded.Length, Is.GreaterThan(HeaderSize));
            Assert.That(encoded[0], Is.EqualTo(Magic[0]));
            Assert.That(encoded[1], Is.EqualTo(Magic[1]));
            Assert.That(encoded[2], Is.EqualTo(Magic[2]));
            Assert.That(encoded[FormatVersionOffset], Is.EqualTo(ExpectedFormatVersion));
            Assert.That(encoded[AlgorithmOffset], Is.EqualTo((byte)EEncryptionAlgorithm.None));
        }

        /// <summary>The checksum in the header is the one the stored payload actually has.</summary>
        [Test]
        public void TheHeaderCarriesTheChecksumOfThePayload()
        {
            byte[] encoded = _codec.Encode(Payload());
            byte[] payload = new byte[encoded.Length - HeaderSize];

            Buffer.BlockCopy(encoded, HeaderSize, payload, 0, payload.Length);

            Assert.That(SaveChecksum.Read(encoded, ChecksumOffset), Is.EqualTo(SaveChecksum.Compute(payload)));
        }

        /// <summary>A file that is not ours is refused before anything tries to parse it.</summary>
        [Test]
        public void SomethingThatIsNotASaveIsRefused()
        {
            byte[] encoded = _codec.Encode(Payload());
            encoded[0] = (byte)'X';

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>Nothing at all is refused rather than walked into.</summary>
        [Test]
        public void NoBytesAtAllAreRefused()
        {
            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(null));
            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(Array.Empty<byte>()));
        }

        /// <summary>A header layout this build does not know is refused, not guessed at.</summary>
        [Test]
        public void AnUnknownHeaderLayoutIsRefused()
        {
            byte[] encoded = _codec.Encode(Payload());
            encoded[FormatVersionOffset] = 99;

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>An algorithm nobody is set up for is refused with an explanation.</summary>
        [Test]
        public void AnAlgorithmWithoutAnEncryptorIsRefused()
        {
            byte[] encoded = _codec.Encode(Payload());
            encoded[AlgorithmOffset] = 99;

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>A payload that no longer matches its checksum is damaged and is refused.</summary>
        [Test]
        public void ADamagedPayloadIsRefused()
        {
            byte[] encoded = _codec.Encode(Payload());
            encoded[^1] = (byte)(encoded[^1] ^ 0xFF);

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>A truncated file cannot even hold its own header and is refused.</summary>
        [Test]
        public void ATruncatedFileIsRefused()
        {
            byte[] encoded = _codec.Encode(Payload());
            byte[] truncated = new byte[LegacyHeaderSize + 1];

            Buffer.BlockCopy(encoded, 0, truncated, 0, truncated.Length);

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(truncated));
        }

        /// <summary>
        /// A file written before the checksum existed still loads, so bumping the header version did
        /// not invalidate saves from an older build.
        /// </summary>
        [Test]
        public void AFileWithoutAChecksumStillLoads()
        {
            SaveProbePayload decoded = _codec.Decode<SaveProbePayload>(LegacyBytes());

            Assert.That(decoded.label, Is.EqualTo(ProbeLabel));
            Assert.That(decoded.score, Is.EqualTo(ProbeScore));
        }

        /// <summary>An encrypted save records that it is encrypted and reads back.</summary>
        [Test]
        public void AnEncryptedSaveSurvivesTheRoundTrip()
        {
            SaveCodec encrypting = EncryptingCodec(_encryptor);

            byte[] encoded = encrypting.Encode(Payload());

            Assert.That(encoded[AlgorithmOffset], Is.EqualTo((byte)EEncryptionAlgorithm.Aes));
            Assert.That(encrypting.Decode<SaveProbePayload>(encoded).score, Is.EqualTo(ProbeScore));
        }

        /// <summary>An encrypted save is unreadable to a build set up with a different passphrase.</summary>
        [Test]
        public void AnEncryptedSaveNeedsTheRightPassphrase()
        {
            byte[] encoded = EncryptingCodec(_encryptor).Encode(Payload());
            SaveCodec other = EncryptingCodec(_otherEncryptor);

            Assert.Catch(() => other.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>An encrypted save is refused by a codec that has no matching encryptor at all.</summary>
        [Test]
        public void AnEncryptedSaveIsRefusedWithoutTheMatchingEncryptor()
        {
            byte[] encoded = EncryptingCodec(_encryptor).Encode(Payload());

            Assert.Throws<InvalidDataException>(() => _codec.Decode<SaveProbePayload>(encoded));
        }

        /// <summary>A codec without a serializer or a write encryptor could never work.</summary>
        [Test]
        public void ACodecNeedsASerializerAndAWriteEncryptor()
        {
            List<ISaveEncryptor> readers = new() { new NoOpEncryptor() };

            Assert.Throws<ArgumentNullException>(() => new SaveCodec(null, new NoOpEncryptor(), readers));
            Assert.Throws<ArgumentNullException>(() => new SaveCodec(new JsonUtilitySerializer(), null, readers));
        }

        /// <summary>The write encryptor can read its own files without being listed twice.</summary>
        [Test]
        public void TheWriteEncryptorCanAlwaysReadBack()
        {
            SaveCodec codec = new(new JsonUtilitySerializer(), new NoOpEncryptor(), new List<ISaveEncryptor>());

            Assert.That(codec.Decode<SaveProbePayload>(codec.Encode(Payload())).score, Is.EqualTo(ProbeScore));
        }

        private static SaveProbePayload Payload() => new()
        {
            label = ProbeLabel,
            score = ProbeScore
        };

        private static SaveCodec PlainCodec() => new(new JsonUtilitySerializer(), new NoOpEncryptor(),
            new List<ISaveEncryptor> { new NoOpEncryptor() });

        private static SaveCodec EncryptingCodec(ISaveEncryptor encryptor)
            => new(new JsonUtilitySerializer(), encryptor, new List<ISaveEncryptor> { encryptor });

        // Format version one had no checksum, so the bytes are assembled by hand rather than by a
        // codec that no longer writes that layout.
        private static byte[] LegacyBytes()
        {
            byte[] payload = Encoding.UTF8.GetBytes(JsonUtility.ToJson(Payload()));
            byte[] result = new byte[LegacyHeaderSize + payload.Length];

            Buffer.BlockCopy(Magic, 0, result, 0, Magic.Length);
            result[FormatVersionOffset] = LegacyFormatVersion;
            result[AlgorithmOffset] = (byte)EEncryptionAlgorithm.None;
            Buffer.BlockCopy(payload, 0, result, LegacyHeaderSize, payload.Length);

            return result;
        }
    }
}