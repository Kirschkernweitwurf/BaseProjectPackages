using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// Finds every data asset field that points to assets about to be deleted, and takes those
    /// references out. A list loses the element instead of keeping an empty slot, because an empty
    /// slot in a collection or a deck is exactly the runtime error deleting should not cause.
    /// </summary>
    /// <remarks>
    /// Only ScriptableObject assets under Assets are searched. Scenes and prefabs keep their reference
    /// and show it as missing, which the confirmation dialog of the delete says.
    /// </remarks>
    internal static class DataReferenceCleaner
    {
        private const char ArrayElementEnd = ']';
        private const string ArrayElementMarker = ".Array.data[";

        /// <summary>
        /// Every field in the project's data assets that points to one of <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The assets about to be deleted.</param>
        /// <param name="index">The assets of the project.</param>
        /// <returns>The references, in the order they were found.</returns>
        internal static List<DataReference> Find(IReadOnlyList<Object> targets, DataAssetIndex index)
        {
            HashSet<Object> targetSet = new(targets);
            HashSet<string> targetPaths = new();

            foreach (Object target in targets)
                targetPaths.Add(AssetDatabase.GetAssetPath(target));

            List<DataReference> references = new();

            foreach (DataAssetEntry entry in index.Entries)
            {
                // The dependency list is cached by the asset database, so it filters out almost every
                // asset before any of them is loaded.
                if (targetPaths.Contains(entry.Path)
                    || !DependsOnAny(entry.Path, targetPaths))
                    continue;

                CollectReferences(AssetDatabase.LoadMainAssetAtPath(entry.Path), targetSet, references);
            }

            return references;
        }

        /// <summary>Removes every reference. Each owner is changed through one undoable step.</summary>
        /// <param name="references">The references to remove, as <see cref="Find"/> returned them.</param>
        internal static void Remove(IReadOnlyList<DataReference> references)
        {
            Dictionary<Object, List<string>> byOwner = new();

            foreach (DataReference reference in references)
            {
                if (!byOwner.TryGetValue(reference.Owner, out List<string> paths))
                {
                    paths = new List<string>();
                    byOwner.Add(reference.Owner, paths);
                }

                paths.Add(reference.PropertyPath);
            }

            foreach (KeyValuePair<Object, List<string>> pair in byOwner)
                RemoveFromOwner(pair.Key, pair.Value);
        }

        private static bool DependsOnAny(string path, HashSet<string> targetPaths)
        {
            foreach (string dependency in AssetDatabase.GetDependencies(path, false))
            {
                if (targetPaths.Contains(dependency))
                    return true;
            }

            return false;
        }

        private static void CollectReferences(Object owner, HashSet<Object> targets, List<DataReference> references)
        {
            if (owner == null)
                return;

            using SerializedObject serialized = new(owner);
            SerializedProperty iterator = serialized.GetIterator();

            while (iterator.Next(true))
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                Object value = iterator.objectReferenceValue;

                if (value != null
                    && targets.Contains(value))
                    references.Add(new DataReference(owner, iterator.propertyPath));
            }
        }

        private static void RemoveFromOwner(Object owner, List<string> paths)
        {
            if (owner == null)
                return;

            using SerializedObject serialized = new(owner);

            // Last found first, so removing an element never shifts the index of one still to remove.
            for (int i = paths.Count - 1; i >= 0; i--)
                RemovePath(serialized, paths[i]);

            serialized.ApplyModifiedProperties();
        }

        private static void RemovePath(SerializedObject serialized, string path)
        {
            if (TrySplitArrayElement(path, out string arrayPath, out int index))
            {
                SerializedProperty array = serialized.FindProperty(arrayPath);

                if (array != null
                    && index < array.arraySize)
                    array.DeleteArrayElementAtIndex(index);

                return;
            }

            SerializedProperty property = serialized.FindProperty(path);

            if (property != null)
                property.objectReferenceValue = null;
        }

        private static bool TrySplitArrayElement(string path, out string arrayPath, out int index)
        {
            arrayPath = null;
            index = 0;

            int marker = path.LastIndexOf(ArrayElementMarker, StringComparison.Ordinal);

            if (marker < 0
                || path[path.Length - 1] != ArrayElementEnd)
                return false;

            int start = marker + ArrayElementMarker.Length;
            string number = path.Substring(start, path.Length - 1 - start);

            if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out index))
                return false;

            arrayPath = path.Substring(0, marker);

            return true;
        }
    }
}