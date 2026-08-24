using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Core
{
    /// <summary>
    /// Holds and draws the arguments of a parameterized inspector button.
    /// </summary>
    /// <remarks>
    /// The values live here rather than in a serialized field, which is the whole point: an argument to
    /// a manual call is not part of the object and should not be saved with it, shipped in a build, or
    /// show up as an override on a prefab. The cost is that they reset on every domain reload, which the
    /// attribute documents.
    /// <para>
    /// Only the types that can be drawn without a SerializedProperty are supported. A parameter of any
    /// other type would need a serialized backing to edit at all, and inventing one behind the scenes is
    /// exactly what this avoids.
    /// </para>
    /// </remarks>
    internal static class ButtonArguments
    {
        private const string KeySeparator = "|";

        private static readonly Dictionary<string, object[]> Values = new();

        // Keyed per property, so the table grows with every field ever touched. Play mode is the point
        // at which none of it matters any more.
        static ButtonArguments() => EditorApplication.playModeStateChanged += _ => Values.Clear();

        /// <summary>Returns whether every parameter of the method can be drawn.</summary>
        /// <param name="method">The method behind the button.</param>
        /// <returns>True when the button can be offered.</returns>
        internal static bool IsSupported(MethodInfo method)
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (!IsSupported(parameter.ParameterType))
                    return false;
            }

            return true;
        }

        /// <summary>Draws a field for each parameter and returns the current values.</summary>
        /// <param name="target">The object the button belongs to.</param>
        /// <param name="method">The method behind the button.</param>
        /// <returns>The argument array to invoke with, or null for a parameterless method.</returns>
        internal static object[] Draw(Object target, MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
                return null;

            object[] values = Resolve(target, method, parameters);

            EditorGUI.indentLevel++;

            for (int i = 0; i < parameters.Length; i++)
            {
                GUIContent label = ScratchContent.For(ObjectNames.NicifyVariableName(parameters[i].Name));
                values[i] = DrawField(label, parameters[i].ParameterType, values[i]);
            }

            EditorGUI.indentLevel--;

            return values;
        }

        private static bool IsSupported(Type type) => type == typeof(int)
            || type == typeof(float)
            || type == typeof(bool)
            || type == typeof(string)
            || type == typeof(Vector2)
            || type == typeof(Vector3)
            || type == typeof(Color)
            || type.IsEnum
            || typeof(Object).IsAssignableFrom(type);

        private static object[] Resolve(Object target, MethodInfo method, ParameterInfo[] parameters)
        {
            string key = target.GetInstanceID()
                + KeySeparator
                + method.DeclaringType?.FullName
                + KeySeparator
                + method.Name;

            if (Values.TryGetValue(key, out object[] cached) && cached.Length == parameters.Length)
                return cached;

            object[] created = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                // A declared default is a better starting point than a zero, since it is the value the
                // author already said the call usually wants.
                created[i] = parameters[i].HasDefaultValue
                    ? parameters[i].DefaultValue
                    : Default(parameters[i].ParameterType);
            }

            Values[key] = created;
            return created;
        }

        private static object Default(Type type) => type.IsValueType
            ? Activator.CreateInstance(type)
            : null;

        private static object DrawField(GUIContent label, Type type, object value)
        {
            if (type == typeof(int))
                return EditorGUILayout.IntField(label, (int)value);

            if (type == typeof(float))
                return EditorGUILayout.FloatField(label, (float)value);

            if (type == typeof(bool))
                return EditorGUILayout.Toggle(label, (bool)value);

            if (type == typeof(string))
                return EditorGUILayout.TextField(label, (string)value);

            if (type == typeof(Vector2))
                return EditorGUILayout.Vector2Field(label, (Vector2)value);

            if (type == typeof(Vector3))
                return EditorGUILayout.Vector3Field(label, (Vector3)value);

            if (type == typeof(Color))
                return EditorGUILayout.ColorField(label, (Color)value);

            if (type.IsEnum)
                return EditorGUILayout.EnumPopup(label, (Enum)value);

            return EditorGUILayout.ObjectField(label, (Object)value, type, true);
        }
    }
}