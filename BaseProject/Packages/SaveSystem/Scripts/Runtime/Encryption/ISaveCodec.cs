namespace Base.SaveSystemPackage.Encryption
{
    /// <summary>
    /// Wraps serialize, encrypt and a small header into one step and back.
    /// Because the header records the encryption mode, a plain dev save and an
    /// encrypted build save can both be loaded by the same codec.
    /// </summary>
    public interface ISaveCodec
    {
        /// <summary>
        /// Encode an object into bytes, including a header and encryption.
        /// </summary>
        /// <param name="value">
        /// The object to encode. Must be serializable by the serializer used in the constructor.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to encode. Must be serializable by the serializer used in the constructor.
        /// </typeparam>
        /// <returns>A byte array containing the header and the (maybe encrypted) serialized object.</returns>
        byte[] Encode<T>(T value);

        /// <summary>
        /// Decode bytes into an object, using the header to determine the encryption mode and deserialization.
        /// </summary>
        /// <param name="bytes">
        /// The byte array to decode. Must have been produced by this codec or a compatible one,
        /// with the expected header and encryption mode.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to decode.
        /// Must be the same type that was encoded or at least compatible with it
        /// and must be deserializable by the serializer used in the constructor.
        /// </typeparam>
        /// <returns>The decoded object of type T.</returns>
        T Decode<T>(byte[] bytes);
    }
}