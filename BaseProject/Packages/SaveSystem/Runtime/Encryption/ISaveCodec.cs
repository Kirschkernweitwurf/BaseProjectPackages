namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Wraps serialize, encrypt and a small header into one step and back. Because the header records
    /// the encryption algorithm, a plain dev save and an encrypted build save can both be loaded by
    /// the same codec.
    /// </summary>
    public interface ISaveCodec
    {
        /// <summary>
        /// Encode an object into bytes, including the header and encryption.
        /// </summary>
        /// <param name="value">The object to encode. Must be handled by the configured serializer.</param>
        /// <typeparam name="T">The type of the object to encode.</typeparam>
        /// <returns>The header followed by the possibly encrypted serialized object.</returns>
        byte[] Encode<T>(T value);

        /// <summary>
        /// Decode bytes into an object, using the header to pick the matching encryptor.
        /// </summary>
        /// <param name="bytes">Bytes produced by this codec or a compatible one.</param>
        /// <typeparam name="T">The type the bytes were encoded from.</typeparam>
        /// <returns>The decoded object.</returns>
        T Decode<T>(byte[] bytes);
    }
}