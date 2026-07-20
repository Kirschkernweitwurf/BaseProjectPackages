using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor.Windows.GetComponentAssigner
{
    /// <summary>
    /// Batch assigns every empty <see cref="GetComponentAttribute"/> and
    /// <see cref="GetComponentInParentAttribute"/> field on prefab assets and on the objects in the open
    /// scenes, so references resolve without opening each inspector once. Mirrors the resolve logic of
    /// the runtime handlers.
    /// </summary>
    internal static class GetComponentBatchAssigner
    {
        private const string ProgressTitle = "Assigning GetComponent references";

        public static int Run(bool includePrefabs, bool includeScenes)
        {
            int assigned = 0;

            try
            {
                if (includePrefabs)
                    assigned += RunPrefabs();

                if (includeScenes)
                    assigned += RunOpenScenes();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }

            return assigned;
        }

        private static int RunPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            int assigned = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (EditorUtility.DisplayCancelableProgressBar(ProgressTitle, path, (float)i / guids.Length))
                    break;

                GameObject root = PrefabUtility.LoadPrefabContents(path);
                int count = ProcessHierarchy(root);

                if (count > 0)
                    PrefabUtility.SaveAsPrefabAsset(root, path);

                PrefabUtility.UnloadPrefabContents(root);
                assigned += count;
            }

            return assigned;
        }

        private static int RunOpenScenes()
        {
            int assigned = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (!scene.isLoaded)
                    continue;

                int count = 0;

                foreach (GameObject root in scene.GetRootGameObjects())
                    count += ProcessHierarchy(root);

                if (count > 0)
                    EditorSceneManager.MarkSceneDirty(scene);

                assigned += count;
            }

            return assigned;
        }

        private static int ProcessHierarchy(GameObject root)
        {
            int assigned = 0;

            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null)
                    assigned += ProcessBehaviour(behaviour);
            }

            return assigned;
        }

        private static int ProcessBehaviour(MonoBehaviour behaviour)
        {
            SerializedObject serializedObject = new(behaviour);
            int assigned = 0;

            foreach (FieldInfo field in ReflectionCache.AllFields(behaviour.GetType()))
            {
                if (!IsReferenceField(field))
                    continue;

                if (TryAssign(behaviour, field, serializedObject))
                    assigned++;
            }

            if (assigned > 0)
                serializedObject.ApplyModifiedProperties();

            return assigned;
        }

        private static bool TryAssign(Component owner, FieldInfo field, SerializedObject serializedObject)
        {
            SerializedProperty property = serializedObject.FindProperty(field.Name);

            if (property == null || property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            if (property.objectReferenceValue != null)
                return false;

            Object found = Resolve(owner, field);

            if (found == null)
                return false;

            property.objectReferenceValue = found;
            return true;
        }

        private static Object Resolve(Component owner, FieldInfo field)
        {
            Type type = field.FieldType;

            if (field.IsDefined(typeof(GetComponentAttribute), false))
                return typeof(Component).IsAssignableFrom(type)
                    ? owner.GetComponent(type)
                    : null;

            GetComponentInParentAttribute inParent = field.GetCustomAttribute<GetComponentInParentAttribute>();

            return inParent != null
                ? SearchParents(owner.transform, type, inParent.Name, inParent.IncludeInactive)
                : null;
        }

        private static Object SearchParents(Transform start, Type type, string name, bool includeInactive)
        {
            for (Transform current = start.parent; current != null; current = current.parent)
            {
                if (!includeInactive && !current.gameObject.activeInHierarchy)
                    continue;

                if (!string.IsNullOrEmpty(name) && current.name != name)
                    continue;

                Object match = Match(current, type);

                if (match != null)
                    return match;
            }

            return null;
        }

        private static Object Match(Transform current, Type type)
        {
            if (type == typeof(Transform))
                return current;

            if (type == typeof(GameObject))
                return current.gameObject;

            return current.GetComponent(type);
        }

        private static bool IsReferenceField(FieldInfo field) => field.IsDefined(typeof(GetComponentAttribute), false)
            || field.IsDefined(typeof(GetComponentInParentAttribute), false);
    }
}