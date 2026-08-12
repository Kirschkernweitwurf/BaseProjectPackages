using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of the exposed parameters of a sibling AudioMixer for
    /// <see cref="MixerParameterAttribute"/>. While the AudioMixer reference is missing or has no
    /// exposed parameters, the plain field stays editable and a compact warning below explains what
    /// is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(MixerParameterAttribute))]
    internal sealed class MixerParameterDrawer : WarningFieldDrawer
    {
        private const string ExposedParametersProperty = "m_ExposedParameters";
        private const string ParameterNameProperty = "name";

        protected override string UsageMessage => AttributeNames.Usage<MixerParameterAttribute>("a string");

        private string[] _names;

        protected override bool IsSupported(SerializedProperty property)
            => property.propertyType == SerializedPropertyType.String;

        protected override string Evaluate(SerializedProperty property)
            => Evaluate(property, (MixerParameterAttribute)attribute, out _names);

        protected override void DrawField(Rect rect, SerializedProperty property, GUIContent label, bool complete)
        {
            if (!complete)
            {
                EditorGUI.PropertyField(rect, property, label);
                return;
            }

            int current = Array.IndexOf(_names, property.stringValue);
            int selected = LabeledField.Popup(rect, label, current, _names);
            if (selected >= 0 && selected < _names.Length && selected != current)
                property.stringValue = _names[selected];
        }

        private static string Evaluate(SerializedProperty property, MixerParameterAttribute attribute,
            out string[] names)
        {
            names = null;

            if (!MemberValueResolver.TryResolveSibling(property, attribute.MixerField, out AudioMixer mixer))
                return $"AudioMixer field '{attribute.MixerField}' was not found on this object.";

            if (mixer == null)
                return $"AudioMixer field '{attribute.MixerField}' is not assigned.";

            names = GetExposedParameters(mixer);
            return names.Length > 0
                ? null
                : "The assigned AudioMixer has no exposed parameters.";
        }

        private static string[] GetExposedParameters(AudioMixer mixer)
        {
            List<string> names = new();

            using SerializedObject serializedMixer = new(mixer);
            SerializedProperty exposed = serializedMixer.FindProperty(ExposedParametersProperty);
            if (exposed == null)
                return Array.Empty<string>();

            for (int i = 0; i < exposed.arraySize; i++)
            {
                SerializedProperty name = exposed.GetArrayElementAtIndex(i)
                    .FindPropertyRelative(ParameterNameProperty);

                if (name != null)
                    names.Add(name.stringValue);
            }

            return names.ToArray();
        }
    }
}