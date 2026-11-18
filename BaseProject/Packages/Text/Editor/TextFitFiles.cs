using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// Loads every prefab and scene that shows a TextMeshPro text onto the stage, directly or through a prefab it
    /// holds, so the sources find their boxes there and every box nobody checks shows up. Environment scenes
    /// without any text stay closed. A list entry, a prefab that gets its size from where it is placed, is measured
    /// there: nested in another prefab or scene or under the parent its <see cref="SpawnedInAttribute"/> names.
    /// </summary>
    internal static class TextFitFiles
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.NonPublic;
        private const string PrefabFilter = "t:Prefab";
        private const string SceneFilter = "t:Scene";
        private const string ScriptFilterFormat = "{0} t:MonoScript";

        private static readonly string[] Folders = { "Assets/Scenes", "Assets/Prefabs" };
        private static readonly Regex NestedPrefab = new(@"m_SourcePrefab: \{fileID: \d+, guid: ([0-9a-f]{32})",
            RegexOptions.Compiled);
        private static readonly string ProjectFolder = Path.GetDirectoryName(Application.dataPath);
        private static readonly Type[] TextTypes = { typeof(TextMeshProUGUI), typeof(TextMeshPro) };

        /// <summary>Loads the prefabs, then the scenes, onto <paramref name="stage"/>.</summary>
        internal static void LoadInto(TextFitStage stage)
        {
            HashSet<string> needles = ScriptGuids();
            HashSet<string> users = PrefabsUsing(needles);
            HashSet<string> placed = PlacedPrefabs();

            foreach (string path in Paths(PrefabFilter))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                // A list entry alone has no size, so it is only measured where it is placed.
                if (users.Contains(guid)
                    && !(placed.Contains(guid) && IsSizedByParent(prefab)))
                    stage.Instantiate(prefab);
            }

            // A scene uses a text itself or through an instance of a prefab that does.
            users.UnionWith(needles);

            foreach (string path in Paths(SceneFilter))
            {
                if (Mentions(Read(path), users))
                    stage.Open(path);
            }

            PlaceSpawned(stage);
        }

        private static bool IsSizedByParent(GameObject prefab) => prefab.transform is RectTransform rect
            && (rect.rect.width <= 0f || rect.rect.height <= 0f);

        // Every prefab nested in another prefab or a scene or placed by code through a field with [SpawnedIn].
        private static HashSet<string> PlacedPrefabs()
        {
            HashSet<string> placed = new();

            foreach (string path in Paths(PrefabFilter))
                AddNested(Read(path), placed);

            foreach (string path in Paths(SceneFilter))
                AddNested(Read(path), placed);

            foreach ((FieldInfo field, _) in SpawnedFields())
            {
                foreach (string path in PrefabsWith(field.DeclaringType))
                {
                    GameObject owner = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (owner == null)
                        continue;

                    foreach (Component component in owner.GetComponentsInChildren(field.DeclaringType, true))
                    {
                        GameObject prefab = PrefabOf(field.GetValue(component));

                        if (prefab != null)
                            placed.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)));
                    }
                }
            }

            return placed;
        }

        private static IEnumerable<string> PrefabsWith(Type type)
        {
            HashSet<string> scripts = new();

            foreach (string guid in AssetDatabase.FindAssets(string.Format(ScriptFilterFormat, type.Name)))
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));

                if (script != null
                    && script.GetClass() == type)
                    scripts.Add(guid);
            }

            foreach (string path in Paths(PrefabFilter))
            {
                if (Mentions(Read(path), scripts))
                    yield return path;
            }
        }

        private static void AddNested(string content, HashSet<string> placed)
        {
            foreach (Match match in NestedPrefab.Matches(content))
                placed.Add(match.Groups[1].Value);
        }

        // Puts every prefab that code places under a parent there, so it gets the room the game gives it.
        private static void PlaceSpawned(TextFitStage stage)
        {
            foreach ((FieldInfo field, FieldInfo parentField) in SpawnedFields())
            {
                foreach (Component component in stage.FindAll(field.DeclaringType))
                {
                    GameObject prefab = PrefabOf(field.GetValue(component));
                    Transform parent = ParentOf(parentField.GetValue(component));

                    if (prefab != null
                        && parent != null)
                        stage.Instantiate(prefab, parent);
                }
            }
        }

        private static IEnumerable<(FieldInfo Field, FieldInfo Parent)> SpawnedFields()
        {
            foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<SpawnedInAttribute>())
            {
                Type owner = field.DeclaringType;

                if (owner == null
                    || !typeof(Component).IsAssignableFrom(owner))
                    continue;

                FieldInfo parent = owner.GetField(field.GetCustomAttribute<SpawnedInAttribute>().ParentField,
                    InstanceFields);

                if (parent != null)
                    yield return (field, parent);
            }
        }

        private static GameObject PrefabOf(object value) => value switch
        {
            GameObject gameObject => gameObject,
            Component component when component != null => component.gameObject,
            _ => null
        };

        // A list of parents, like one group per section, places the entry in its first one.
        private static Transform ParentOf(object value)
        {
            if (value is Transform transform)
                return transform;

            if (value is not IEnumerable list)
                return null;

            foreach (object item in list)
            {
                if (item is Transform first
                    && first != null)
                    return first;
            }

            return null;
        }

        // From the project folder, since a plugin may change the working directory a relative path depends on.
        private static string Read(string assetPath) => File.ReadAllText(Path.Combine(ProjectFolder, assetPath));

        private static HashSet<string> ScriptGuids()
        {
            HashSet<string> guids = new();

            foreach (Type type in TextTypes)
            {
                foreach (string guid in AssetDatabase.FindAssets(string.Format(ScriptFilterFormat, type.Name)))
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));

                    if (script != null
                        && script.GetClass() == type)
                        guids.Add(guid);
                }
            }

            return guids;
        }

        private static IEnumerable<string> Paths(string filter)
        {
            foreach (string guid in AssetDatabase.FindAssets(filter, Folders))
                yield return AssetDatabase.GUIDToAssetPath(guid);
        }

        // Reading the files is far cheaper than opening them. Repeats until no new prefab joins, so a prefab that
        // nests another one with a text counts too.
        private static HashSet<string> PrefabsUsing(HashSet<string> scriptGuids)
        {
            Dictionary<string, string> files = new();

            foreach (string path in Paths(PrefabFilter))
                files[AssetDatabase.AssetPathToGUID(path)] = Read(path);

            HashSet<string> users = new();
            HashSet<string> needles = new(scriptGuids);
            bool hasGrown = true;

            while (hasGrown)
            {
                hasGrown = false;

                foreach (KeyValuePair<string, string> file in files)
                {
                    if (users.Contains(file.Key)
                        || !Mentions(file.Value, needles))
                        continue;

                    users.Add(file.Key);
                    needles.Add(file.Key);
                    hasGrown = true;
                }
            }

            return users;
        }

        private static bool Mentions(string content, IEnumerable<string> guids)
        {
            foreach (string guid in guids)
            {
                if (content.Contains(guid))
                    return true;
            }

            return false;
        }
    }
}