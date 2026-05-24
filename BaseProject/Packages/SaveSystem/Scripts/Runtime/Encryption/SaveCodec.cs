using System;
using System.Collections.Generic;
using System.IO;
using ISaveSerializer = Base.SaveSystemPackage.Serialization.ISaveSerializer;

namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Adds a tiny header to save files to identify them and their encryption mode,
    /// so the same code can read both dev and build saves without guessing or separate folders.
    ///
    /// <code>
    ///   [ 'B','S','V' ]  magic, 3 bytes      -> "is this even our file?"
    ///   [ formatVersion ] 1 byte             -> header layout version
    ///   [ encryptionMode ] 1 byte            -> which ISaveEncryptor wrote it
    ///   [ payload ... ]                      -> (maybe encrypted) serialized bytes
    /// </code>
    /// </summary>
    public sealed class SaveCodec : ISaveCodec
    {
        private const byte FormatVersion = 1;
        private const int HeaderSize = 5;

        private static readonly byte[] Magic = { (byte)'B', (byte)'S', (byte)'V' };

        private readonly ISaveSerializer _serializer;
        private readonly ISaveEncryptor _writeEncryptor;
        private readonly Dictionary<ESaveEncryption, ISaveEncryptor> _readEncryptors = new();

        /// <summary>
        /// Create a new SaveCodec.
        /// </summary>
        /// <param name ="serializer">Used for the actual serialization of objects.
        /// The codec just adds a header and encryption on top of that.</param>
        /// <param name="writeEncryptor">Used when saving. Use <see cref="NoOpEncryptor"/> for plain saves.</param>
        /// <param name="readEncryptors">All encryptors that might be needed to read.
        /// Always include NoOp + AES so you can read both dev and build saves.</param>
        public SaveCodec(ISaveSerializer serializer, ISaveEncryptor writeEncryptor,
            IEnumerable<ISaveEncryptor> readEncryptors)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _writeEncryptor = writeEncryptor ?? throw new ArgumentNullException(nameof(writeEncryptor));

            foreach (ISaveEncryptor enc in readEncryptors)
                if (enc != null)
                    _readEncryptors[enc.Mode] = enc;

            _readEncryptors[_writeEncryptor.Mode] = _writeEncryptor;
        }

        /// <inheritdoc />
        public byte[] Encode<T>(T value)
        {
            byte[] payload = _writeEncryptor.Encrypt(_serializer.Serialize(value));

            byte[] result = new byte[HeaderSize + payload.Length];
            result[0] = Magic[0];
            result[1] = Magic[1];
            result[2] = Magic[2];
            result[3] = FormatVersion;
            result[4] = (byte)_writeEncryptor.Mode;
            Buffer.BlockCopy(payload, 0, result, HeaderSize, payload.Length);

            return result;
        }

        /// <inheritdoc />
        public T Decode<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length < HeaderSize ||
                bytes[0] != Magic[0] || bytes[1] != Magic[1] || bytes[2] != Magic[2])
                throw new InvalidDataException("Not a valid save file (bad header).");

            ESaveEncryption mode = (ESaveEncryption)bytes[4];

            if (!_readEncryptors.TryGetValue(mode, out ISaveEncryptor encryptor))
                throw new InvalidDataException(
                    $"Save uses encryption mode '{mode}', but no matching encryptor is set up. " +
                    "Make sure the same passphrase/encryptors are configured.");

            byte[] payload = new byte[bytes.Length - HeaderSize];
            Buffer.BlockCopy(bytes, HeaderSize, payload, 0, payload.Length);

            return _serializer.Deserialize<T>(encryptor.Decrypt(payload));
        }
    }
}