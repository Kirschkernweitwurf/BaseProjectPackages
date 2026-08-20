using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws an AudioMixerGroup field as a dropdown of a mixer's groups for
    /// <see cref="AudioMixerGroupAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(AudioMixerGroupAttribute))]
    internal sealed class AudioMixerGroupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                LabeledField.Hint(position, label,
                    AttributeNames.Usage<AudioMixerGroupAttribute>("an AudioMixerGroup field"));

                return;
            }

            AudioMixerGroupAttribute attribute = (AudioMixerGroupAttribute)this.attribute;
            AudioMixer mixer = ResolveMixer(property, attribute.MixerField);
            AudioMixerGroup[] groups = mixer == null
                ? null
                : mixer.FindMatchingGroups(string.Empty);

            if (groups == null || groups.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] names = CollectNames(groups, property, out int current);

            EditorGUI.BeginProperty(position, label, property);
            int selected = LabeledField.Popup(position, label, current, names);
            if (selected >= 0 && selected < groups.Length && selected != current)
                property.objectReferenceValue = groups[selected];

            EditorGUI.EndProperty();
        }

        private static string[] CollectNames(AudioMixerGroup[] groups, SerializedProperty property, out int current)
        {
            AudioMixerGroup currentGroup = property.objectReferenceValue as AudioMixerGroup;
            string[] names = new string[groups.Length];
            current = -1;

            for (int i = 0; i < groups.Length; i++)
            {
                names[i] = groups[i].name;
                if (groups[i] == currentGroup)
                    current = i;
            }

            return names;
        }

        private static AudioMixer ResolveMixer(SerializedProperty property, string mixerField)
        {
            if (MemberValueResolver.TryResolveSibling(property, mixerField, out AudioMixer fromField)
                && fromField != null)
                return fromField;

            AudioMixerGroup currentGroup = property.objectReferenceValue as AudioMixerGroup;
            return currentGroup != null
                ? currentGroup.audioMixer
                : null;
        }
    }
}