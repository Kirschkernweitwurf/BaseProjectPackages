using System.Collections.Generic;
using Base.ControllerSupport.Controller.Navigation;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// The single rebuild entry point for <see cref="NavigableGroup"/>s, used by the inspector buttons
    /// and the <see cref="NavigationGroupsWindow"/>. A rebuild adds missing elements, rewires the
    /// navigation and marks the touched selectables dirty. Rebuilds are always triggered deliberately
    /// by the user, never automatically, so wiring changes are visible instead of happening silently.
    /// </summary>
    public static class NavigationRebuildService
    {
        private const string PrefabFilter = "t:Prefab";
        private const string ProgressTitle = "Rebuilding Navigation";
        private const string SceneFilter = "t:Scene";
        private const float ScenePhaseShare = 0.5f;

        private static readonly List<NavigableElement> Elements = new();
        private static readonly List<NavigableGroup> Groups = new();
        private static readonly string[] SearchFolders =
        {
            "Assets"
        };

        /// <summary>Rebuilds a single group and marks everything it touched dirty.</summary>
        public static void RebuildGroup(NavigableGroup group)
        {
            if (group == null)
            {
                CustomLogger.LogWarning("Cannot rebuild a null group.", null);
                return;
            }

            NavigationValidator.AddMissingElements(group.transform);
            group.Rebuild();
            MarkElementsDirty(group);
        }

        /// <summary>Rebuilds every group in the loaded scenes, including the ones on inactive objects.</summary>
        public static void RebuildLoadedScenes()
        {
            int rebuilt = RebuildFoundGroups();
            CustomLogger.Log($"Rebuilt {rebuilt} navigable group(s) in the loaded scenes.", null);
        }

        /// <summary>
        /// Rebuilds navigation in every scene and prefab of the project and saves the results. Opens
        /// each scene one by one and restores the current scene setup afterward. Does nothing when the
        /// user cancels over unsaved changes.
        /// </summary>
        public static void RebuildProject()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            int sceneGroups;
            int prefabGroups;

            try
            {
                sceneGroups = RebuildAllScenes();
                prefabGroups = RebuildAllPrefabs();
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            CustomLogger.Log($"Project rebuild done: {sceneGroups} group(s) across all scenes, "
                + $"{prefabGroups} group(s) across all prefabs.", null);
        }

        private static int RebuildAllScenes()
        {
            string[] guids = AssetDatabase.FindAssets(SceneFilter, SearchFolders);
            int rebuilt = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(ProgressTitle, path, i / (float)guids.Length * ScenePhaseShare);

                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                int groups = RebuildFoundGroups();

                if (groups == 0)
                    continue;

                rebuilt += groups;
                EditorSceneManager.SaveOpenScenes();
            }

            return rebuilt;
        }

        private static int RebuildAllPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets(PrefabFilter, SearchFolders);
            int rebuilt = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                float progress = ScenePhaseShare + i / (float)guids.Length * (1f - ScenePhaseShare);
                EditorUtility.DisplayProgressBar(ProgressTitle, path, progress);

                // Cheap asset check first, so only prefabs that actually carry groups are opened for edit.
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (asset == null
                    || asset.GetComponentInChildren<NavigableGroup>(true) == null)
                    continue;

                rebuilt += RebuildPrefab(path);
            }

            return rebuilt;
        }

        private static int RebuildPrefab(string path)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(path);

            try
            {
                List<NavigableGroup> groups = new();
                contents.GetComponentsInChildren(true, groups);

                foreach (NavigableGroup group in groups)
                    RebuildGroup(group);

                PrefabUtility.SaveAsPrefabAsset(contents, path);
                return groups.Count;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static int RebuildFoundGroups()
        {
            NavigableGroup[] found = Object.FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            // Copied out first, because adding missing elements changes the scene while iterating.
            Groups.Clear();
            Groups.AddRange(found);

            foreach (NavigableGroup group in Groups)
                RebuildGroup(group);

            return Groups.Count;
        }

        private static void MarkElementsDirty(NavigableGroup group)
        {
            Elements.Clear();
            group.GetComponentsInChildren(true, Elements);

            foreach (NavigableElement element in Elements)
            {
                if (element.Selectable != null)
                    EditorUtility.SetDirty(element.Selectable);
            }
        }
    }
}