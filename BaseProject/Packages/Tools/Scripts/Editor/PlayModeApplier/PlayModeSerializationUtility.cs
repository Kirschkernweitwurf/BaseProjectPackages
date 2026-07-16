using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Turns a live object into a payload and back again.
    /// Two things make this non trivial: Unity's own internal fields must never be overwritten,
    /// and object references are stored as session local instance ids that die with the play mode scene.
    /// </summary>
    public static class PlayModeSerializationUtility
    {
        private const int FieldDepth = 2;

        /// <summary>
        /// Fields owned by Unity that describe identity or hierarchy. Writing these back would
        /// detach the component from its GameObject or corrupt the prefab link.
        /// </summary>
        private static readonly HashSet<string> InternalKeys = new()
        {
            "m_ObjectHideFlags",
            "m_CorrespondingSourceObject",
            "m_PrefabInstance",
            "m_PrefabAsset",
            "m_GameObject",
            "m_EditorHideFlags",
            "m_Script",
            "m_EditorClassIdentifier",
            "m_Component",
            "m_Father",
            "m_Children"
        };

        /// <summary>Serializes the object, including private serialized fields, to JSON.</summary>
        public static string CaptureJson(Object target) => EditorJsonUtility.ToJson(target);

        /// <summary>
        /// Records what every non null object reference points at, so it can be restored in edit mode
        /// instead of pointing at a dead play mode instance id.
        /// Pass the instance root when capturing a runtime instantiated object, so that references between
        /// its own parts are stored relative to it and survive the write into the prefab asset.
        /// </summary>
        public static List<PlayModeObjectReference> CaptureObjectReferences(Object target, Transform instanceRoot)
        {
            List<PlayModeObjectReference> references = new();
            SerializedObject serializedObject = new(target);
            SerializedProperty iterator = serializedObject.GetIterator();

            while (iterator.Next(true))
            {
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (IsInternalPath(iterator.propertyPath))
                    continue;

                Object value = iterator.objectReferenceValue;
                if (value == null)
                    continue;

                references.Add(BuildReference(iterator.propertyPath, value, instanceRoot));
            }

            serializedObject.Dispose();
            return references;
        }

        /// <summary>Writes captured JSON onto the target, skipping Unity's internal identity fields.</summary>
        public static void ApplyJson(Object target, string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            EditorJsonUtility.FromJsonOverwrite(StripInternalKeys(json), target);
        }

        /// <summary>
        /// Re-resolves every recorded reference against the edit mode scenes and asset database.
        /// Runs after <see cref="ApplyJson"/>, so array sizes already match the captured state.
        /// Pass the loaded prefab root when writing into a prefab asset, otherwise pass null.
        /// </summary>
        public static void ApplyObjectReferences(Object target, List<PlayModeObjectReference> references,
            Transform prefabRoot)
        {
            if (references == null
                || references.Count == 0)
                return;

            SerializedObject serializedObject = new(target);

            foreach (PlayModeObjectReference reference in references)
            {
                SerializedProperty property = serializedObject.FindProperty(reference.propertyPath);
                if (property == null)
                    continue;

                property.objectReferenceValue = ResolveReference(reference, target, prefabRoot);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Dispose();
        }

        private static PlayModeObjectReference BuildReference(string propertyPath, Object value, Transform instanceRoot)
        {
            PlayModeObjectReference reference = new() { propertyPath = propertyPath };

            if (EditorUtility.IsPersistent(value))
            {
                reference.kind = EPlayModeReferenceKind.Asset;
                reference.globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();
                return reference;
            }

            Transform owner = ResolveOwner(value, reference);
            if (owner == null)
            {
                reference.kind = EPlayModeReferenceKind.Unresolvable;
                return reference;
            }

            if (instanceRoot != null && IsDescendantOf(owner, instanceRoot))
            {
                reference.kind = EPlayModeReferenceKind.PrefabInternal;
                reference.namePath = PlayModeObjectLocator.BuildRelativeNamePath(instanceRoot, owner);
                reference.indexPath = PlayModeObjectLocator.BuildRelativeIndexPath(instanceRoot, owner);
                return reference;
            }

            if (PlayModePrefabResolver.FindCloneRoot(owner) != null)
            {
                reference.kind = EPlayModeReferenceKind.Unresolvable;
                return reference;
            }

            reference.kind = EPlayModeReferenceKind.SceneObject;
            reference.scenePath = owner.gameObject.scene.path;
            reference.namePath = PlayModeObjectLocator.BuildSceneNamePath(owner);
            reference.indexPath = PlayModeObjectLocator.BuildSceneIndexPath(owner);
            return reference;
        }

        private static Transform ResolveOwner(Object value, PlayModeObjectReference reference)
        {
            GameObject gameObject = value as GameObject;
            if (gameObject != null)
                return gameObject.transform;

            Component component = value as Component;
            if (component == null)
                return null;

            reference.componentTypeName = component.GetType().FullName;
            reference.componentIndex = PlayModeObjectLocator.FindComponentIndex(component);
            return component.transform;
        }

        private static bool IsDescendantOf(Transform target, Transform root)
        {
            Transform cursor = target;
            while (cursor != null)
            {
                if (cursor == root)
                    return true;

                cursor = cursor.parent;
            }

            return false;
        }

        private static Object ResolveReference(PlayModeObjectReference reference, Object owner, Transform prefabRoot)
        {
            if (reference.kind == EPlayModeReferenceKind.Asset)
                return ResolveAsset(reference);

            if (reference.kind == EPlayModeReferenceKind.PrefabInternal)
            {
                if (prefabRoot == null)
                    return null;

                return PickObject(PlayModeObjectLocator.ResolveFromRoot(prefabRoot, reference.namePath,
                    reference.indexPath), reference);
            }

            if (reference.kind == EPlayModeReferenceKind.SceneObject)
                return ResolveSceneObject(reference, owner, prefabRoot);

            Debug.LogWarning(
                $"Play Mode Saver cleared '{reference.propertyPath}' because it pointed at a runtime object.", owner);
            return null;
        }

        private static Object ResolveAsset(PlayModeObjectReference reference)
        {
            if (!GlobalObjectId.TryParse(reference.globalObjectId, out GlobalObjectId identifier))
                return null;

            return GlobalObjectId.GlobalObjectIdentifierToObjectSlow(identifier);
        }

        private static Object ResolveSceneObject(PlayModeObjectReference reference, Object owner, Transform prefabRoot)
        {
            if (prefabRoot != null)
            {
                Debug.LogWarning(
                    $"Play Mode Saver cleared '{reference.propertyPath}' because a prefab cannot hold a " +
                    "reference to a scene object.", owner);
                return null;
            }

            Scene scene = SceneManager.GetSceneByPath(reference.scenePath);
            if (!scene.IsValid()
                || !scene.isLoaded)
            {
                Debug.LogWarning(
                    $"Play Mode Saver cleared '{reference.propertyPath}' because '{reference.scenePath}' is not open.",
                    owner);
                return null;
            }

            return PickObject(PlayModeObjectLocator.ResolveInScene(scene, reference.namePath, reference.indexPath),
                reference);
        }

        private static Object PickObject(Transform owner, PlayModeObjectReference reference)
        {
            if (owner == null)
                return null;

            if (string.IsNullOrEmpty(reference.componentTypeName))
                return owner.gameObject;

            return PlayModeObjectLocator.ResolveComponent(owner, reference.componentTypeName,
                reference.componentIndex);
        }

        private static bool IsInternalPath(string propertyPath)
        {
            foreach (string key in InternalKeys)
            {
                if (propertyPath == key || propertyPath.StartsWith(key + "."))
                    return true;
            }

            return false;
        }

        private static string StripInternalKeys(string json)
        {
            StringBuilder builder = new(json.Length);
            int index = 0;
            int depth = 0;

            while (index < json.Length)
            {
                char current = json[index];

                if (current == '"')
                {
                    int stringEnd = FindStringEnd(json, index);
                    if (depth != FieldDepth || !IsKeyPosition(json, index))
                    {
                        builder.Append(json, index, stringEnd - index + 1);
                        index = stringEnd + 1;
                        continue;
                    }

                    string key = json.Substring(index + 1, stringEnd - index - 1);
                    int valueEnd = FindValueEnd(json, stringEnd + 1);

                    if (InternalKeys.Contains(key))
                    {
                        index = SkipRemovedField(json, valueEnd, builder);
                        continue;
                    }

                    builder.Append(json, index, valueEnd - index);
                    index = valueEnd;
                    continue;
                }

                if (current == '{' || current == '[')
                    depth++;
                else if (current == '}' || current == ']')
                    depth--;

                builder.Append(current);
                index++;
            }

            return builder.ToString();
        }

        private static bool IsKeyPosition(string json, int quoteIndex)
        {
            int cursor = quoteIndex - 1;
            while (cursor >= 0 && char.IsWhiteSpace(json[cursor]))
                cursor--;

            if (cursor < 0)
                return false;

            return json[cursor] == '{' || json[cursor] == ',';
        }

        private static int FindStringEnd(string json, int startQuote)
        {
            int cursor = startQuote + 1;
            while (cursor < json.Length)
            {
                char current = json[cursor];
                if (current == '\\')
                {
                    cursor += 2;
                    continue;
                }

                if (current == '"')
                    return cursor;

                cursor++;
            }

            return json.Length - 1;
        }

        private static int FindValueEnd(string json, int afterKey)
        {
            int cursor = afterKey;
            while (cursor < json.Length && json[cursor] != ':')
                cursor++;

            cursor++;
            while (cursor < json.Length && char.IsWhiteSpace(json[cursor]))
                cursor++;

            if (cursor >= json.Length)
                return json.Length;

            char current = json[cursor];
            if (current == '"')
                return FindStringEnd(json, cursor) + 1;

            if (current == '{' || current == '[')
                return FindContainerEnd(json, cursor) + 1;

            while (cursor < json.Length && json[cursor] != ',' && json[cursor] != '}' && json[cursor] != ']')
                cursor++;

            return cursor;
        }

        private static int FindContainerEnd(string json, int start)
        {
            int cursor = start;
            int depth = 0;

            while (cursor < json.Length)
            {
                char current = json[cursor];
                if (current == '"')
                {
                    cursor = FindStringEnd(json, cursor) + 1;
                    continue;
                }

                if (current == '{' || current == '[')
                {
                    depth++;
                }
                else if (current == '}' || current == ']')
                {
                    depth--;
                    if (depth == 0)
                        return cursor;
                }

                cursor++;
            }

            return json.Length - 1;
        }

        private static int SkipRemovedField(string json, int valueEnd, StringBuilder builder)
        {
            int cursor = valueEnd;
            while (cursor < json.Length && char.IsWhiteSpace(json[cursor]))
                cursor++;

            if (cursor < json.Length && json[cursor] == ',')
                return cursor + 1;

            TrimTrailingComma(builder);
            return cursor;
        }

        private static void TrimTrailingComma(StringBuilder builder)
        {
            int cursor = builder.Length - 1;
            while (cursor >= 0 && char.IsWhiteSpace(builder[cursor]))
                cursor--;

            if (cursor < 0 || builder[cursor] != ',')
                return;

            builder.Remove(cursor, 1);
        }
    }
}
