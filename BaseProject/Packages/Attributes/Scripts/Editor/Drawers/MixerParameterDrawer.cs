using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.References;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>
    /// Draws a dropdown of the exposed parameters of a sibling AudioMixer for
    /// <see cref="MixerParameterAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(MixerParameterAttribute))]
    public sealed class MixerParameterDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [MixerParameter] with a string.");
                return;
            }

            MixerParameterAttribute attribute = (MixerParameterAttribute)this.attribute;
            AudioMixer mixer = ResolveMixer(property, attribute.MixerField);
            if (mixer == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            List<string> names = GetExposedParameters(mixer);
            if (names.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            int current = names.IndexOf(property.stringValue);
            int selected = EditorGUI.Popup(position, label.text, current, names.ToArray());
            if (selected >= 0 && selected < names.Count)
                property.stringValue = names[selected];

            EditorGUI.EndProperty();
        }

        private static AudioMixer ResolveMixer(SerializedProperty property, string mixerField)
        {
            Object target = property.serializedObject.targetObject;
            FieldInfo field = ReflectionCache.GetField(target.GetType(), mixerField);
            return field?.GetValue(target) as AudioMixer;
        }

        private static List<string> GetExposedParameters(AudioMixer mixer)
        {
            List<string> names = new();

            SerializedObject serializedMixer = new(mixer);
            SerializedProperty exposed = serializedMixer.FindProperty("m_ExposedParameters");
            if (exposed != null)
                for (int i = 0; i < exposed.arraySize; i++)
                {
                    SerializedProperty name = exposed.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                    if (name != null)
                        names.Add(name.stringValue);
                }

            return names;
        }
    }
}