using System;
using Base.SaveSystemPackage.Encryption;
using NUnit.Framework;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the value that decides whether a save file is trusted. The hash has to change when the
    /// bytes do, and the four bytes it is stored in have to read back the same on any machine.
    /// </summary>
    public sealed class SaveChecksumTests
    {
        private const uint OffsetBasis = 2166136261u;

        /// <summary>The same bytes hash to the same value every time.</summary>
        [Test]
        public void TheSamePayloadHashesTheSame()
        {
            byte[] payload =
            {
                1,
                2,
                3,
                4
            };

            Assert.That(SaveChecksum.Compute(payload), Is.EqualTo(SaveChecksum.Compute(payload)));
        }

        /// <summary>A single changed byte has to change the hash, or damage goes unnoticed.</summary>
        [Test]
        public void AChangedByteChangesTheHash()
        {
            byte[] original =
            {
                1,
                2,
                3,
                4
            };

            byte[] damaged =
            {
                1,
                2,
                9,
                4
            };

            Assert.That(SaveChecksum.Compute(damaged), Is.Not.EqualTo(SaveChecksum.Compute(original)));
        }

        /// <summary>Reordered bytes are different data and hash differently.</summary>
        [Test]
        public void ReorderedBytesHashDifferently()
        {
            byte[] original =
            {
                1,
                2
            };

            byte[] swapped =
            {
                2,
                1
            };

            Assert.That(SaveChecksum.Compute(swapped), Is.Not.EqualTo(SaveChecksum.Compute(original)));
        }

        /// <summary>No payload is not an error, it is the starting value of the hash.</summary>
        [Test]
        public void AMissingPayloadHashesToTheOffsetBasis()
        {
            Assert.That(SaveChecksum.Compute(null), Is.EqualTo(OffsetBasis));
            Assert.That(SaveChecksum.Compute(Array.Empty<byte>()), Is.EqualTo(OffsetBasis));
        }

        /// <summary>A written checksum reads back as the value that was written.</summary>
        [Test]
        public void AWrittenChecksumReadsBackUnchanged()
        {
            byte[] buffer = new byte[SaveChecksum.Length];

            SaveChecksum.Write(uint.MaxValue, buffer, 0);

            Assert.That(SaveChecksum.Read(buffer, 0), Is.EqualTo(uint.MaxValue));
        }

        /// <summary>The value is stored least significant byte first, on every machine.</summary>
        [Test]
        public void TheChecksumIsStoredLeastSignificantByteFirst()
        {
            byte[] buffer = new byte[SaveChecksum.Length];

            SaveChecksum.Write(0x04030201u, buffer, 0);

            Assert.That(buffer, Is.EqualTo(new byte[]
            {
                0x01,
                0x02,
                0x03,
                0x04
            }));
        }

        /// <summary>Writing at an offset leaves the bytes around it alone.</summary>
        [Test]
        public void WritingAtAnOffsetLeavesTheRestAlone()
        {
            byte[] buffer = new byte[SaveChecksum.Length + 2];
            buffer[0] = 0xFF;

            SaveChecksum.Write(1u, buffer, 1);

            Assert.That(buffer[0], Is.EqualTo(0xFF));
            Assert.That(SaveChecksum.Read(buffer, 1), Is.EqualTo(1u));
        }
    }
}