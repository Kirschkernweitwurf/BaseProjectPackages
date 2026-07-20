using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws an AudioMixerGroup field as a dropdown of a mixer's groups for
    /// <see cref="AudioMixerGroupAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(AudioMixerGroupAttribute))]
    public sealed class AudioMixerGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text,
                    AttributeNames.Usage<AudioMixerGroupAttribute>("an AudioMixerGroup field"));
                return;
            }

            AudioMixerGroupAttribute attribute = (AudioMixerGroupAttribute)this.attribute;
            AudioMixer mixer = ResolveMixer(property, attribute.MixerField);
            if (mixer == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AudioMixerGroup[] groups = mixer.FindMatchingGroups(string.Empty);
            if (groups == null || groups.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            AudioMixerGroup currentGroup = property.objectReferenceValue as AudioMixerGroup;
            string[] names = new string[groups.Length];
            int current = -1;
            for (int i = 0; i < groups.Length; i++)
            {
                names[i] = groups[i].name;
                if (groups[i] == currentGroup)
                    current = i;
            }

            EditorGUI.BeginProperty(position, label, property);
            int selected = EditorGUI.Popup(position, label.text, current, names);
            if (selected >= 0 && selected < groups.Length && selected != current)
                property.objectReferenceValue = groups[selected];

            EditorGUI.EndProperty();
        }

        private static AudioMixer ResolveMixer(SerializedProperty property, string mixerField)
        {
            if (!string.IsNullOrEmpty(mixerField))
            {
                Object target = property.serializedObject.targetObject;
                FieldInfo field = ReflectionCache.GetField(target.GetType(), mixerField);
                if (field?.GetValue(target) is AudioMixer fromField)
                    return fromField;
            }

            AudioMixerGroup currentGroup = property.objectReferenceValue as AudioMixerGroup;
            return currentGroup != null
                ? currentGroup.audioMixer
                : null;
        }
    }
}