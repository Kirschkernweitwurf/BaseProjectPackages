using System.Collections.Generic;
using Base.DataHubPackage.Editor.AssetActions;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.Validation
{
    /// <summary>
    /// Which assets other data assets or scenes refer to. Data assets are read as they are loaded, not from
    /// the saved dependency data, so a card added to a deck counts as used before the deck is saved. Scenes
    /// are read from the dependency data, once per rebuild, because they change rarely and are expensive to
    /// open. Collections are left out: every card is in its collection, which says nothing about whether it
    /// is played.
    /// </summary>
    internal sealed class DataUsageIndex
    {
        private readonly DataAssetIndex _assets;
        private readonly DataHubRegistry _registry;

        private const string ScenesFilter = "t:Scene";

        private HashSet<string> _sceneDependencies;
        private HashSet<Object> _used;

        /// <summary>Creates the index over the project's data assets.</summary>
        /// <param name="assets">The assets of the project.</param>
        /// <param name="registry">The extensions, to tell collections apart.</param>
        internal DataUsageIndex(DataAssetIndex assets, DataHubRegistry registry)
        {
            _assets = assets;
            _registry = registry;
        }

        /// <summary>Forgets what the data assets hold, after an edit. Scenes are kept until the next rebuild.</summary>
        internal void Clear() => _used = null;

        /// <summary>Forgets everything, after the project changed on disk.</summary>
        internal void ClearAll()
        {
            _used = null;
            _sceneDependencies = null;
        }

        /// <summary>True when another data asset that is not a collection refers to <paramref name="asset"/>.</summary>
        /// <param name="asset">The asset to look for.</param>
        /// <returns>True when it is used.</returns>
        internal bool IsUsed(Object asset)
        {
            _used ??= Collect();
            _sceneDependencies ??= CollectSceneDependencies();

            return _used.Contains(asset)
                || _sceneDependencies.Contains(AssetDatabase.GetAssetPath(asset));
        }

        private HashSet<Object> Collect()
        {
            HashSet<Object> used = new();

            foreach (DataAssetEntry entry in _assets.Entries)
            {
                if (_registry.IsCollectionType(entry.Type))
                    continue;

                Object owner = AssetDatabase.LoadMainAssetAtPath(entry.Path);

                if (owner != null)
                    CollectReferences(owner, used);
            }

            return used;
        }

        private static HashSet<string> CollectSceneDependencies()
        {
            List<string> scenes = new();

            foreach (string guid in AssetDatabase.FindAssets(ScenesFilter, new[] { DataAssetActions.AssetsRoot }))
                scenes.Add(AssetDatabase.GUIDToAssetPath(guid));

            return new HashSet<string>(AssetDatabase.GetDependencies(scenes.ToArray(), true));
        }

        private static void CollectReferences(Object owner, HashSet<Object> used)
        {
            using SerializedObject serialized = new(owner);
            SerializedProperty iterator = serialized.GetIterator();

            while (iterator.Next(true))
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                Object value = iterator.objectReferenceValue;

                // A reference to itself, like a card pointing at its own upgrade slot, is no use by others.
                if (value != null
                    && value != owner)
                    used.Add(value);
            }
        }
    }
}