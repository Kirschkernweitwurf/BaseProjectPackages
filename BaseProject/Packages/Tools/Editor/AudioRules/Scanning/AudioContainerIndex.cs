using System.Collections.Generic;
using Base.ToolPackage.Editor.AudioRules.Data;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.AudioRules.Scanning
{
    /// <summary>
    /// Reads the audio containers of the project and remembers which category each clip was
    /// referenced with and whether anything loops it. Purely optional context: a project without
    /// containers scans exactly the same, the category conditions just never match.
    /// </summary>
    internal sealed class AudioContainerIndex
    {
        private const string TypeFilterPrefix = "t:";

        private readonly Dictionary<string, string> _categoryByGuid = new();
        private readonly HashSet<string> _loopingGuids = new();

        /// <summary>How many clips a container was found for.</summary>
        internal int ReferencedClips => _categoryByGuid.Count;

        /// <summary>Reads every container the rule set binds to.</summary>
        /// <param name="ruleSet">The rule set holding the bindings.</param>
        /// <returns>The filled index.</returns>
        internal static AudioContainerIndex Build(AudioRuleSet ruleSet)
        {
            AudioContainerIndex index = new();

            foreach (AudioContainerBinding binding in ruleSet.ContainerBindings)
            {
                if (binding.IsUsable())
                    index.ReadBinding(binding);
            }

            return index;
        }

        /// <summary>The category a clip was referenced with.</summary>
        /// <param name="clipGuid">GUID of the clip.</param>
        /// <returns>The category name, or an empty string when nothing references the clip.</returns>
        internal string GetCategory(string clipGuid) => _categoryByGuid.GetValueOrDefault(clipGuid, string.Empty);

        /// <summary>True when a container plays the clip as a loop.</summary>
        /// <param name="clipGuid">GUID of the clip.</param>
        /// <returns>True when at least one container loops it.</returns>
        internal bool IsLooping(string clipGuid) => _loopingGuids.Contains(clipGuid);

        /// <summary>True when at least one container references the clip.</summary>
        /// <param name="clipGuid">GUID of the clip.</param>
        /// <returns>True when the clip is referenced.</returns>
        internal bool HasContainer(string clipGuid) => _categoryByGuid.ContainsKey(clipGuid);

        private static string ReadCategory(SerializedProperty property)
        {
            if (property == null)
                return string.Empty;

            if (property.propertyType == SerializedPropertyType.String)
                return property.stringValue;

            if (property.propertyType != SerializedPropertyType.Enum)
                return string.Empty;

            // The numeric value is the project's own business, the entry name is what a rule matches.
            string[] names = property.enumNames;
            int index = property.enumValueIndex;

            return index >= 0 && index < names.Length
                ? names[index]
                : string.Empty;
        }

        private static bool ReadLoop(SerializedProperty property)
            => property != null && property.propertyType == SerializedPropertyType.Boolean && property.boolValue;

        private void ReadBinding(AudioContainerBinding binding)
        {
            foreach (string guid in AssetDatabase.FindAssets(TypeFilterPrefix + binding.TypeName))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object container = AssetDatabase.LoadMainAssetAtPath(path);

                if (container == null)
                    continue;

                ReadContainer(container, binding);
            }
        }

        private void ReadContainer(Object container, AudioContainerBinding binding)
        {
            using SerializedObject serialized = new(container);

            string category = ReadCategory(SerializedFieldLookup.Find(serialized, binding.CategoryField));
            bool loops = ReadLoop(SerializedFieldLookup.Find(serialized, binding.LoopField));
            SerializedProperty clips = SerializedFieldLookup.Find(serialized, binding.ClipsField);

            if (clips == null)
                return;

            if (clips.propertyType == SerializedPropertyType.ObjectReference)
            {
                Register(clips.objectReferenceValue, category, loops);
                return;
            }

            if (!clips.isArray)
                return;

            for (int index = 0; index < clips.arraySize; index++)
                Register(clips.GetArrayElementAtIndex(index).objectReferenceValue, category, loops);
        }

        private void Register(Object clip, string category, bool loops)
        {
            if (clip == null)
                return;

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));

            if (string.IsNullOrEmpty(guid))
                return;

            // First container wins the category. A clip in two categories is rare enough that
            // silently picking one beats making every caller deal with a list.
            if (!_categoryByGuid.ContainsKey(guid))
                _categoryByGuid[guid] = category;

            if (loops)
                _loopingGuids.Add(guid);
        }
    }
}