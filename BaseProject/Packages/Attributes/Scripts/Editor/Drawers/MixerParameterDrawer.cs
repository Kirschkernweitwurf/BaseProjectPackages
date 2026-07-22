using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a dropdown of the exposed parameters of a sibling AudioMixer for
    /// <see cref="MixerParameterAttribute"/>. While the AudioMixer reference is missing or has no
    /// exposed parameters, the plain field stays editable and a compact warning below explains what
    /// is missing.
    /// </summary>
    [CustomPropertyDrawer(typeof(MixerParameterAttribute))]
    public sealed class MixerParameterDrawer : PropertyDrawer
    {
        private const string ExposedParametersProperty = "m_ExposedParameters";
        private const string ParameterNameProperty = "name";
        private const float WarningSpacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return EditorGUIUtility.singleLineHeight;

            string warning = Evaluate(property, (MixerParameterAttribute)attribute, out _);
            return warning == null
                ? EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight + WarningSpacing + CompactHelpBox.Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, AttributeNames.Usage<MixerParameterAttribute>("a string"));
                return;
            }

            string warning = Evaluate(property, (MixerParameterAttribute)attribute, out string[] names);

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, property);

            if (warning == null)
            {
                int current = Array.IndexOf(names, property.stringValue);
                int selected = EditorGUI.Popup(fieldRect, label.text, current, names);
                if (selected >= 0 && selected < names.Length && selected != current)
                    property.stringValue = names[selected];
            }
            else
            {
                EditorGUI.PropertyField(fieldRect, property, label);
            }

            EditorGUI.EndProperty();

            if (warning == null)
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                CompactHelpBox.Height);

            CompactHelpBox.Draw(warningRect, warning, EInfoBoxType.Warning);
        }

        private static string Evaluate(SerializedProperty property, MixerParameterAttribute attribute,
            out string[] names)
        {
            names = null;

            if (!TryResolveMixer(property, attribute.MixerField, out AudioMixer mixer))
                return $"AudioMixer field '{attribute.MixerField}' was not found on this object.";

            if (mixer == null)
                return $"AudioMixer field '{attribute.MixerField}' is not assigned.";

            names = GetExposedParameters(mixer);
            return names.Length > 0
                ? null
                : "The assigned AudioMixer has no exposed parameters.";
        }

        private static bool TryResolveMixer(SerializedProperty property, string mixerField, out AudioMixer mixer)
        {
            mixer = null;
            Object target = property.serializedObject.targetObject;
            FieldInfo field = ReflectionCache.GetField(target.GetType(), mixerField);
            if (field == null || field.FieldType != typeof(AudioMixer))
                return false;

            mixer = field.GetValue(target) as AudioMixer;
            return true;
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
                SerializedProperty name = exposed.GetArrayElementAtIndex(i).FindPropertyRelative(ParameterNameProperty);
                if (name != null)
                    names.Add(name.stringValue);
            }

            return names.ToArray();
        }
    }
}