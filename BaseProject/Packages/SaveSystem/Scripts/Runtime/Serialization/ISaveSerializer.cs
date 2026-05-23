namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// Turns objects into bytes and back.
    /// </summary>
    public interface ISaveSerializer
    {
        /// <summary>
        /// Turns an object into bytes. The type <c>T</c> must be the same when you deserialize it later.
        /// </summary>
        /// <param name="value">The object to serialize. Must be serializable by the implementation you choose.</param>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <returns>A byte array representing the serialized object.</returns>
        byte[] Serialize<T>(T value);

        /// <summary>
        /// Turns bytes back into an object. The type <c>T</c> must be the same as when you serialized it.
        /// </summary>
        /// <param name="bytes">
        /// The byte array to deserialize.
        /// Must have been produced by the same implementation's <see cref="Serialize{T}"/> method.
        /// </param>
        /// <typeparam name="T">
        /// The type of the object to deserialize. Must be the same as when you serialized it.
        /// </typeparam>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(byte[] bytes);
    }
}