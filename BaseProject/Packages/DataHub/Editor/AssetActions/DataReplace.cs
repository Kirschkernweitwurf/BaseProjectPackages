using System;
using System.Collections.Generic;
using System.Reflection;
using Base.DataHubPackage.Editor.Lists;
using Base.DataHubPackage.Editor.Table;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.DataHubPackage.Editor.AssetActions
{
    /// <summary>
    /// Swaps one asset for another in every list and field of the project's data assets, like a card that is
    /// replaced by a new version in every deck. Lists keep their order and their counts; a collection that
    /// already holds the new asset only loses the old one, so it never ends up with a copy twice.
    /// </summary>
    internal static class DataReplace
    {
        private const string ArrayElementMarker = ".Array.data[";
        private const string ArraySegment = "Array";
        private const char PathSeparator = '.';
        private const string UndoFormat = "Replace {0}";

        /// <summary>Every list and field that holds <paramref name="asset"/>, one line each.</summary>
        /// <param name="asset">The asset to replace.</param>
        /// <param name="index">The assets of the project.</param>
        /// <param name="registry">The extensions, to tell collection lists apart.</param>
        /// <returns>The lines, in the order they were found.</returns>
        internal static List<DataReplaceLine> Plan(Object asset, DataAssetIndex index, DataHubRegistry registry)
        {
            List<DataReplaceLine> lines = new();
            Dictionary<string, DataReplaceLine> byKey = new();

            foreach (DataReference reference in DataReferenceCleaner.Find(new[] { asset }, index))
            {
                int marker = reference.PropertyPath.LastIndexOf(ArrayElementMarker, StringComparison.Ordinal);
                bool isList = marker >= 0;

                string path = isList
                    ? reference.PropertyPath.Substring(0, marker)
                    : reference.PropertyPath;

                string key = reference.Owner.GetInstanceID() + path;

                if (!byKey.TryGetValue(key, out DataReplaceLine line))
                {
                    line = new DataReplaceLine(reference.Owner, path, LabelOf(path),
                        SlotTypeAt(reference.Owner.GetType(), reference.PropertyPath), isList,
                        isList && registry.FindCollectionRule(reference.Owner.GetType(), path) != null);

                    byKey.Add(key, line);
                    lines.Add(line);
                }

                line.Count++;
            }

            return lines;
        }

        /// <summary>Notes which lists already hold the replacement, which decides how a collection changes.</summary>
        /// <param name="lines">The lines of <see cref="Plan"/>.</param>
        /// <param name="replacement">The new asset, or null.</param>
        internal static void Prepare(IReadOnlyList<DataReplaceLine> lines, Object replacement)
        {
            foreach (DataReplaceLine line in lines)
            {
                line.HasReplacement = false;

                if (replacement == null
                    || !line.IsList)
                    continue;

                using SerializedObject serialized = new(line.Owner);
                SerializedProperty list = serialized.FindProperty(line.Path);

                if (list != null)
                    line.HasReplacement = DataListEditing.Count(list, replacement) > 0;
            }
        }

        /// <summary>Applies the replacement to every line it fits, as one undo step.</summary>
        /// <param name="lines">The lines of <see cref="Plan"/>, prepared for the replacement.</param>
        /// <param name="asset">The asset to replace.</param>
        /// <param name="replacement">The new asset, or null to remove the old one.</param>
        /// <returns>How many slots changed.</returns>
        internal static int Apply(IReadOnlyList<DataReplaceLine> lines, Object asset, Object replacement)
        {
            int group = Undo.GetCurrentGroup();
            int changed = 0;

            foreach (DataReplaceLine line in lines)
            {
                if (line.Owner == null
                    || !line.Fits(replacement))
                    continue;

                using SerializedObject serialized = new(line.Owner);
                SerializedProperty property = serialized.FindProperty(line.Path);

                if (property == null)
                    continue;

                changed += line.IsList
                    ? ApplyToList(property, asset, replacement, line.IsRemoval(replacement))
                    : ApplyToField(property, asset, replacement);

                serialized.ApplyModifiedProperties();
            }

            Undo.SetCurrentGroupName(string.Format(UndoFormat, asset.name));
            Undo.CollapseUndoOperations(group);

            return changed;
        }

        // From the end, so removing an element never shifts one still to visit.
        private static int ApplyToList(SerializedProperty list, Object asset, Object replacement, bool isRemoval)
        {
            int changed = 0;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);

                if (element.objectReferenceValue != asset)
                    continue;

                if (isRemoval)
                    list.DeleteArrayElementAtIndex(i);
                else
                    element.objectReferenceValue = replacement;

                changed++;
            }

            return changed;
        }

        private static int ApplyToField(SerializedProperty field, Object asset, Object replacement)
        {
            if (field.objectReferenceValue != asset)
                return 0;

            field.objectReferenceValue = replacement;

            return 1;
        }

        private static string LabelOf(string path)
        {
            int separator = path.LastIndexOf(PathSeparator);

            string name = separator < 0
                ? path
                : path.Substring(separator + 1);

            return DataColumnLabel.Clean(name);
        }

        // Walks the serialized path through the field types, so a slot's accepted type is known without
        // writing to it. Returns null where the walk cannot follow, which then allows any replacement.
        private static Type SlotTypeAt(Type ownerType, string propertyPath)
        {
            Type current = ownerType;
            string[] segments = propertyPath.Split(PathSeparator);

            for (int i = 0; i < segments.Length && current != null; i++)
            {
                if (segments[i] == ArraySegment)
                {
                    current = DataSerializedFields.ElementType(current);
                    i++;

                    continue;
                }

                current = FieldType(current, segments[i]);
            }

            return current;
        }

        private static Type FieldType(Type owner, string name)
        {
            foreach (FieldInfo field in DataSerializedFields.Of(owner))
            {
                if (field.Name == name)
                    return field.FieldType;
            }

            return null;
        }
    }
}