using System.Text;
using UnityEngine;

namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// Serializer built on Unity's built-in <see cref="JsonUtility"/>.
    /// </summary>
    public sealed class JsonUtilitySerializer : ISaveSerializer
    {
        private readonly bool _prettyPrint;

        /// <param name="prettyPrint">Indent the JSON so a save file can be read and edited by hand.</param>
        public JsonUtilitySerializer(bool prettyPrint = false) => _prettyPrint = prettyPrint;

        /// <inheritdoc/>
        public byte[] Serialize<T>(T value)
        {
            string json = JsonUtility.ToJson(value, _prettyPrint);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }
    }
}