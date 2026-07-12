using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Resolves the managed object behind a <see cref="SerializedProperty"/> so the handler pipeline
    /// can descend into nested serializable types. Walks the property path with reflection and
    /// handles array and list elements.
    /// </summary>
    public static class SerializedPropertyReflection
    {
        private const string ArrayToken = "Array";
        private const string DataPrefix = "data[";

        /// <summary>Returns the managed value of the given property, or null when it cannot resolve.</summary>
        public static object GetValue(SerializedProperty property)
        {
            object current = property.serializedObject.targetObject;
            string[] elements = property.propertyPath.Split('.');

            int index = 0;
            while (index < elements.Length && current != null)
                index = Step(elements, index, ref current);

            return current;
        }

        private static int Step(string[] elements, int index, ref object current)
        {
            string element = elements[index];

            if (element == ArrayToken && index + 1 < elements.Length && elements[index + 1].StartsWith(DataPrefix))
            {
                current = GetArrayElement(current, ParseIndex(elements[index + 1]));
                return index + 2;
            }

            current = GetFieldValue(current, element);
            return index + 1;
        }

        private static object GetFieldValue(object source, string name)
        {
            if (source == null)
                return null;

            FieldInfo field = ReflectionCache.GetField(source.GetType(), name);
            return field?.GetValue(source);
        }

        private static object GetArrayElement(object source, int arrayIndex)
        {
            if (source is not IList list || arrayIndex < 0 || arrayIndex >= list.Count)
                return null;

            return list[arrayIndex];
        }

        private static int ParseIndex(string data)
        {
            int start = data.IndexOf('[') + 1;
            int end = data.IndexOf(']');
            return int.TryParse(data.Substring(start, end - start), out int value)
                ? value
                : -1;
        }
    }
}