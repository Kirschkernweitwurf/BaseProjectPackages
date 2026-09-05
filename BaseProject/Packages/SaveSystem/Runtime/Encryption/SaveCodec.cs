using System;
using System.Collections.Generic;
using System.IO;
using Base.SaveSystemPackage.Serialization;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Adds a tiny header to save files to identify them and their encryption algorithm, so the same
    /// code can read both dev and build saves without guessing or separate folders. The checksum turns
    /// a truncated or edited file into a clean "this save is damaged" instead of a confusing parse
    /// error somewhere further down.
    /// <code>
    ///   [ 'B','S','V' ]   magic, 3 bytes    -> "is this even our file?"
    ///   [ formatVersion ] 1 byte            -> header layout version
    ///   [ algorithm ]     1 byte            -> which ISaveEncryptor wrote it
    ///   [ checksum ]      4 bytes           -> FNV-1a of the payload, format version 2 and up
    ///   [ payload ... ]                     -> the possibly encrypted serialized bytes
    /// </code>
    /// </summary>
    /// <remarks>
    /// Format version 1 had no checksum. Those files are still read, they simply cannot be checked, so
    /// bumping the version does not invalidate saves written by an older build.
    /// </remarks>
    public sealed class SaveCodec : ISaveCodec
    {
        private const int AlgorithmOffset = 4;
        private const int ChecksumOffset = 5;
        private const byte FormatVersion = 2;
        private const int FormatVersionOffset = 3;
        private const int HeaderSize = 9;
        private const byte LegacyFormatVersion = 1;
        private const int LegacyHeaderSize = 5;
        private const int MagicLength = 3;

        private static readonly byte[] Magic =
        {
            (byte)'B',
            (byte)'S',
            (byte)'V'
        };

        private readonly ISaveSerializer _serializer;
        private readonly ISaveEncryptor _writeEncryptor;
        private readonly Dictionary<EEncryptionAlgorithm, ISaveEncryptor> _readEncryptors = new();

        /// <param name="serializer">
        /// Does the actual serialization. The codec only adds a header and encryption on top.
        /// </param>
        /// <param name="writeEncryptor">Used when saving. Use <see cref="NoOpEncryptor"/> for plain saves.</param>
        /// <param name="readEncryptors">
        /// Every encryptor that might be needed to read. Include NoOp and AES so both dev and build
        /// saves can be loaded.
        /// </param>
        /// <exception cref="ArgumentNullException">When any of the three arguments is null.</exception>
        public SaveCodec(ISaveSerializer serializer, ISaveEncryptor writeEncryptor,
            IEnumerable<ISaveEncryptor> readEncryptors)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _writeEncryptor = writeEncryptor ?? throw new ArgumentNullException(nameof(writeEncryptor));

            if (readEncryptors == null)
                throw new ArgumentNullException(nameof(readEncryptors));

            foreach (ISaveEncryptor encryptor in readEncryptors)
            {
                if (encryptor != null)
                    _readEncryptors[encryptor.Algorithm] = encryptor;
            }

            _readEncryptors[_writeEncryptor.Algorithm] = _writeEncryptor;
        }

        /// <inheritdoc/>
        public byte[] Encode<T>(T value)
        {
            byte[] payload = _writeEncryptor.Encrypt(_serializer.Serialize(value));
            byte[] result = new byte[HeaderSize + payload.Length];

            Buffer.BlockCopy(Magic, 0, result, 0, MagicLength);
            result[FormatVersionOffset] = FormatVersion;
            result[AlgorithmOffset] = (byte)_writeEncryptor.Algorithm;

            // Over the payload as it is stored, so the check works without the key being right.
            SaveChecksum.Write(SaveChecksum.Compute(payload), result, ChecksumOffset);

            Buffer.BlockCopy(payload, 0, result, HeaderSize, payload.Length);

            return result;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">
        /// When the header is missing, its layout version is unknown, no encryptor matches, or the
        /// payload does not match its checksum.
        /// </exception>
        public T Decode<T>(byte[] bytes)
        {
            byte formatVersion = ReadFormatVersion(bytes);
            bool isLegacy = formatVersion == LegacyFormatVersion;

            int headerSize = isLegacy
                ? LegacyHeaderSize
                : HeaderSize;

            if (bytes.Length < headerSize)
                throw new InvalidDataException("Save file is too short to hold its own header.");

            EEncryptionAlgorithm algorithm = (EEncryptionAlgorithm)bytes[AlgorithmOffset];
            if (!_readEncryptors.TryGetValue(algorithm, out ISaveEncryptor encryptor))
                throw new InvalidDataException($"Save was written with '{algorithm}', but no matching encryptor "
                    + "is set up. Make sure the same passphrase and encryptors are configured.");

            byte[] payload = new byte[bytes.Length - headerSize];
            Buffer.BlockCopy(bytes, headerSize, payload, 0, payload.Length);

            // Files written before the checksum existed are read on trust rather than rejected.
            if (!isLegacy)
                VerifyChecksum(payload, SaveChecksum.Read(bytes, ChecksumOffset));

            return _serializer.Deserialize<T>(encryptor.Decrypt(payload));
        }

        private static byte ReadFormatVersion(byte[] bytes)
        {
            if (!HasValidMagic(bytes))
                throw new InvalidDataException("Not a valid save file (bad header).");

            byte formatVersion = bytes[FormatVersionOffset];

            if (formatVersion != FormatVersion && formatVersion != LegacyFormatVersion)
                throw new InvalidDataException($"Save header format version {formatVersion} is not supported "
                    + $"(expected {LegacyFormatVersion} or {FormatVersion}).");

            return formatVersion;
        }

        private static void VerifyChecksum(byte[] payload, uint expected)
        {
            uint actual = SaveChecksum.Compute(payload);

            if (actual != expected)
                throw new InvalidDataException("Save file is damaged: its contents do not match the checksum "
                    + "written with them.");
        }

        private static bool HasValidMagic(byte[] bytes)
        {
            if (bytes == null || bytes.Length < LegacyHeaderSize)
                return false;

            for (int i = 0; i < MagicLength; i++)
            {
                if (bytes[i] != Magic[i])
                    return false;
            }

            return true;
        }
    }
}