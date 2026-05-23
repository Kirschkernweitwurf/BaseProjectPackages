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

        public JsonUtilitySerializer(bool prettyPrint = false) => _prettyPrint = prettyPrint;

        public byte[] Serialize<T>(T value)
        {
            string json = JsonUtility.ToJson(value, _prettyPrint);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }
    }
}