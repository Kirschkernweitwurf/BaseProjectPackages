using System;
using System.Collections.Generic;
using System.IO;
using Base.SaveSystemPackage.Serialization;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Adds a tiny header to save files to identify them and their encryption algorithm, so the same
    /// code can read both dev and build saves without guessing or separate folders.
    /// <code>
    ///   [ 'B','S','V' ]   magic, 3 bytes    -> "is this even our file?"
    ///   [ formatVersion ] 1 byte            -> header layout version
    ///   [ algorithm ]     1 byte            -> which ISaveEncryptor wrote it
    ///   [ payload ... ]                     -> the possibly encrypted serialized bytes
    /// </code>
    /// </summary>
    public sealed class SaveCodec : ISaveCodec
    {
        private const int AlgorithmOffset = 4;
        private const byte FormatVersion = 1;
        private const int FormatVersionOffset = 3;
        private const int HeaderSize = 5;
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
        /// <exception cref="ArgumentNullException">When the serializer or the write encryptor is null.</exception>
        public SaveCodec(ISaveSerializer serializer, ISaveEncryptor writeEncryptor,
            IEnumerable<ISaveEncryptor> readEncryptors)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _writeEncryptor = writeEncryptor ?? throw new ArgumentNullException(nameof(writeEncryptor));

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
            Buffer.BlockCopy(payload, 0, result, HeaderSize, payload.Length);

            return result;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidDataException">
        /// When the header is missing, its layout version is unknown, or no encryptor matches.
        /// </exception>
        public T Decode<T>(byte[] bytes)
        {
            if (!HasValidMagic(bytes))
                throw new InvalidDataException("Not a valid save file (bad header).");

            byte formatVersion = bytes[FormatVersionOffset];
            if (formatVersion != FormatVersion)
                throw new InvalidDataException(
                    $"Save header format version {formatVersion} is not supported (expected {FormatVersion}).");

            EEncryptionAlgorithm algorithm = (EEncryptionAlgorithm)bytes[AlgorithmOffset];
            if (!_readEncryptors.TryGetValue(algorithm, out ISaveEncryptor encryptor))
                throw new InvalidDataException($"Save was written with '{algorithm}', but no matching encryptor "
                    + "is set up. Make sure the same passphrase and encryptors are configured.");

            byte[] payload = new byte[bytes.Length - HeaderSize];
            Buffer.BlockCopy(bytes, HeaderSize, payload, 0, payload.Length);

            return _serializer.Deserialize<T>(encryptor.Decrypt(payload));
        }

        private static bool HasValidMagic(byte[] bytes)
        {
            if (bytes == null || bytes.Length < HeaderSize)
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