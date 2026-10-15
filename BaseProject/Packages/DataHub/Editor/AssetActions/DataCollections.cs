using System;
using System.Collections.Generic;
using Base.DataHubPackage.Editor.Extensions;
using Base.UtilityPackage.Logging;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// The collection assets the create rules point to: finding them, telling whether an asset is in
    /// one, and adding it. Lookups are cached until the project changes, so validating a few hundred
    /// rows reads each collection once.
    /// </summary>
    internal sealed class DataCollections
    {
        private readonly Dictionary<Type, Object> _collections = new();
        private readonly DataAssetIndex _index;
        private readonly Dictionary<Object, HashSet<Object>> _members = new();

        /// <summary>Creates the lookup over the project's assets.</summary>
        /// <param name="index">The assets of the project.</param>
        internal DataCollections(DataAssetIndex index) => _index = index;

        /// <summary>Forgets every cached lookup. Call when the project changed.</summary>
        internal void Clear()
        {
            _collections.Clear();
            _members.Clear();
        }

        /// <summary>The collection asset of <paramref name="rule"/>, or null when it has none.</summary>
        /// <param name="rule">The create rule, may be null.</param>
        /// <returns>The collection asset, or null.</returns>
        internal Object Find(IDataCreateRule rule)
        {
            if (rule?.CollectionType == null)
                return null;

            if (_collections.TryGetValue(rule.CollectionType, out Object cached))
                return cached;

            Object collection = Locate(rule.CollectionType);
            _collections.Add(rule.CollectionType, collection);

            return collection;
        }

        /// <summary>
        /// True when <paramref name="rule"/> names a collection and <paramref name="asset"/> is not in it.
        /// A missing or broken collection was already reported and counts as not missing, so every row
        /// does not repeat the same complaint.
        /// </summary>
        /// <param name="rule">The create rule of the asset, may be null.</param>
        /// <param name="asset">The asset to look for.</param>
        /// <param name="collection">The collection that was searched.</param>
        /// <returns>True when the asset should be in the collection but is not.</returns>
        internal bool IsMissingFrom(IDataCreateRule rule, Object asset, out Object collection)
        {
            collection = Find(rule);

            if (collection == null)
                return false;

            HashSet<Object> members = Members(collection, rule.CollectionListProperty);

            return members != null
                && !members.Contains(asset);
        }

        /// <summary>
        /// Adds <paramref name="asset"/> to the collection of <paramref name="rule"/>, if it has one.
        /// </summary>
        /// <param name="rule">The create rule of the asset, may be null.</param>
        /// <param name="asset">The asset to add.</param>
        internal void Add(IDataCreateRule rule, Object asset)
        {
            Object collection = Find(rule);

            if (collection == null)
                return;

            using SerializedObject serialized = new(collection);
            SerializedProperty list = serialized.FindProperty(rule.CollectionListProperty);

            if (!IsObjectList(list))
            {
                LogMissingList(collection, rule.CollectionListProperty);
                return;
            }

            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = asset;
            serialized.ApplyModifiedProperties();

            _members.Remove(collection);
        }

        private static bool IsObjectList(SerializedProperty list) => list != null
            && list.isArray
            && list.arrayElementType.StartsWith("PPtr", StringComparison.Ordinal);

        private static void LogMissingList(Object collection, string listProperty)
            => CustomLogger.LogError($"{collection.name} has no object list named '{listProperty}'. Check the "
                + $"{nameof(IDataCreateRule.CollectionListProperty)} of its create rule.", collection);

        private Object Locate(Type collectionType)
        {
            List<string> paths = new();
            _index.CollectPaths(collectionType, paths);

            if (paths.Count == 0)
            {
                CustomLogger.LogWarning($"No {collectionType.Name} asset found. New assets join no collection "
                    + "until one exists.", null);

                return null;
            }

            Object collection = AssetDatabase.LoadMainAssetAtPath(paths[0]);

            if (paths.Count > 1)
                CustomLogger.LogWarning($"Found {paths.Count} {collectionType.Name} assets. The Data Hub uses "
                    + $"{collection.name}.", collection);

            return collection;
        }

        private HashSet<Object> Members(Object collection, string listProperty)
        {
            if (_members.TryGetValue(collection, out HashSet<Object> cached))
                return cached;

            using SerializedObject serialized = new(collection);
            SerializedProperty list = serialized.FindProperty(listProperty);

            HashSet<Object> members = null;

            if (IsObjectList(list))
            {
                members = new HashSet<Object>();

                for (int i = 0; i < list.arraySize; i++)
                    members.Add(list.GetArrayElementAtIndex(i).objectReferenceValue);
            }
            else
            {
                LogMissingList(collection, listProperty);
            }

            // Cached even when broken, so the error shows once per project change and not once per row.
            _members.Add(collection, members);

            return members;
        }
    }
}