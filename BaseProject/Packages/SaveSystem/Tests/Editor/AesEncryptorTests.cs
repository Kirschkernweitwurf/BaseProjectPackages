using System;
using System.IO;
using System.Text;
using Base.SaveSystemPackage.Encryption;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the encryption a shipped build writes with. The same save must never look the same
    /// twice on disk, and a file must be unreadable to anyone without the passphrase it was written
    /// with.
    /// </summary>
    /// <remarks>
    /// Deriving a key runs a hundred thousand hashing rounds, so the encryptors are built once for the
    /// whole fixture rather than per test.
    /// </remarks>
    public sealed class AesEncryptorTests
    {
        private const string OtherPassphrase = "a different passphrase";
        private const string Salt = "a salt of our own";
        private const string Passphrase = "the passphrase";
        private const string PlainText = "The quick brown fox jumps over the lazy dog.";

        private AesEncryptor _encryptor;
        private AesEncryptor _other;
        private AesEncryptor _salted;
        private byte[] _plain;

        /// <summary>Derives every key once, since that is the expensive part.</summary>
        [OneTimeSetUp]
        public void BuildKeys()
        {
            _encryptor = new AesEncryptor(Passphrase);
            _other = new AesEncryptor(OtherPassphrase);
            _salted = new AesEncryptor(Passphrase, Encoding.UTF8.GetBytes(Salt));
            _plain = Encoding.UTF8.GetBytes(PlainText);
        }

        /// <summary>The encryptor reports which algorithm wrote the file.</summary>
        [Test]
        public void TheAlgorithmIsReported()
            => Assert.That(_encryptor.Algorithm, Is.EqualTo(EEncryptionAlgorithm.Aes));

        /// <summary>What was encrypted comes back byte for byte.</summary>
        [Test]
        public void TheDataSurvivesTheRoundTrip()
        {
            byte[] cipher = _encryptor.Encrypt(_plain);

            Assert.That(_encryptor.Decrypt(cipher), Is.EqualTo(_plain));
        }

        /// <summary>The cipher is not the plain text sitting in a different wrapper.</summary>
        [Test]
        public void TheCipherDoesNotContainThePlainText()
        {
            byte[] cipher = _encryptor.Encrypt(_plain);

            Assert.That(Encoding.UTF8.GetString(cipher), Does.Not.Contain(PlainText));
        }

        /// <summary>
        /// The same data encrypted twice looks different, because a fresh initialization vector is
        /// drawn every time. Without that, two saves of the same state would be visibly identical.
        /// </summary>
        [Test]
        public void TheSameDataLooksDifferentEveryTime()
        {
            byte[] first = _encryptor.Encrypt(_plain);
            byte[] second = _encryptor.Encrypt(_plain);

            Assert.That(second, Is.Not.EqualTo(first));
            Assert.That(_encryptor.Decrypt(second), Is.EqualTo(_plain), "both still decrypt to the same data");
        }

        /// <summary>Empty data is legal and comes back empty.</summary>
        [Test]
        public void EmptyDataSurvivesTheRoundTrip()
        {
            byte[] cipher = _encryptor.Encrypt(Array.Empty<byte>());

            Assert.That(_encryptor.Decrypt(cipher), Is.Empty);
        }

        /// <summary>A file written with another passphrase cannot be read.</summary>
        [Test]
        public void AnotherPassphraseCannotRead()
        {
            byte[] cipher = _encryptor.Encrypt(_plain);

            Assert.That(DecryptOrNull(_other, cipher), Is.Not.EqualTo(_plain));
        }

        /// <summary>A custom salt makes a different key out of the same passphrase.</summary>
        [Test]
        public void ACustomSaltChangesTheKey()
        {
            byte[] cipher = _salted.Encrypt(_plain);

            Assert.That(_salted.Decrypt(cipher), Is.EqualTo(_plain));
            Assert.That(DecryptOrNull(_encryptor, cipher), Is.Not.EqualTo(_plain));
        }

        /// <summary>Data too short to hold an initialization vector is refused.</summary>
        [Test]
        public void DataTooShortToHoldAnInitializationVectorIsRefused()
        {
            Assert.Throws<InvalidDataException>(() => _encryptor.Decrypt(null));
            Assert.Throws<InvalidDataException>(() => _encryptor.Decrypt(Array.Empty<byte>()));
            Assert.Throws<InvalidDataException>(() => _encryptor.Decrypt(new byte[16]));
        }

        /// <summary>A passphrase has to exist, since the key is derived from it.</summary>
        [Test]
        public void APassphraseIsRequired()
        {
            Assert.Throws<ArgumentException>(() => new AesEncryptor(null));
            Assert.Throws<ArgumentException>(() => new AesEncryptor(string.Empty));
        }

        // Decrypting with the wrong key almost always fails on the padding, but every so often the
        // padding happens to look valid and garbage comes back instead. Both count as unreadable, so
        // the failure is folded into a result the assertion can compare.
        private static byte[] DecryptOrNull(AesEncryptor encryptor, byte[] cipher)
        {
            try
            {
                return encryptor.Decrypt(cipher);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}