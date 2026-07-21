#if UNITY_EDITOR
using System.Collections.Generic;
using Base.ControllerSupport.Controller.Navigation;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// Shared rebuild entry points for <see cref="NavigableGroup"/>s, used by the inspector buttons
    /// and the <see cref="NavigationGroupsWindow"/>. Rebuilds are always triggered deliberately by the
    /// user, never automatically, so wiring changes are visible instead of happening silently.
    /// </summary>
    public static class NavigationRebuildService
    {
        private const string ProgressTitle = "Rebuilding Navigation";

        /// <summary>Rebuilds navigation on every group in the loaded scenes, including inactive ones.</summary>
        public static void RebuildLoadedScenes()
        {
            NavigableGroup[] groups = Object.FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (NavigableGroup group in groups)
                group.Rebuild();

            CustomLogger.Log($"Rebuilt {groups.Length} navigable group(s) in the loaded scenes.", null);
        }

        /// <summary>
        /// Rebuilds navigation in every scene and prefab of the project and saves the results. Opens
        /// each scene one by one and restores the current scene setup afterwards. Returns false when
        /// the user cancelled over unsaved changes.
        /// </summary>
        public static bool RebuildProject()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            int sceneGroups = 0;
            int prefabGroups = 0;

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

            return true;
        }

        private static int RebuildAllScenes()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[]
            {
                "Assets"
            });

            int rebuilt = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(ProgressTitle, path, i / (float)guids.Length * 0.5f);

                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                NavigableGroup[] groups = Object.FindObjectsByType<NavigableGroup>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

                if (groups.Length == 0)
                    continue;

                foreach (NavigableGroup group in groups)
                    group.Rebuild();

                rebuilt += groups.Length;
                EditorSceneManager.SaveOpenScenes();
            }

            return rebuilt;
        }

        private static int RebuildAllPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[]
            {
                "Assets"
            });

            int rebuilt = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(ProgressTitle, path, 0.5f + i / (float)guids.Length * 0.5f);

                // Cheap asset check first, so only prefabs that actually carry groups are opened for edit.
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null || asset.GetComponentInChildren<NavigableGroup>(true) == null)
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
                    group.Rebuild();

                PrefabUtility.SaveAsPrefabAsset(contents, path);
                return groups.Count;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
#endif